using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyApi.Application;
using MyApi.Data;
using MyApi.Infrastructure.ExceptionHandling;
using MyApi.Infrastructure.HealthChecks;
using MyApi.Infrastructure.RateLimiting;
using MyApi.Infrastructure.Secrets;
using MyApi.Infrastructure.Security;
using MyApi.Middlewarex;
using MyApi.Models;
using MyApi.Options;
using MyApi.Services;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.OpenTelemetry;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Activity.ForceDefaultIdFormat = true;

var builder = WebApplication.CreateBuilder(args);
ApplyEnvironmentOverrides(builder.Configuration);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()
    ?? new CorsSettings();
var rateLimitingSettings = builder.Configuration.GetSection(RateLimitingSettings.SectionName).Get<RateLimitingSettings>()
    ?? new RateLimitingSettings();
var observabilitySettings = builder.Configuration.GetSection(ObservabilitySettings.SectionName).Get<ObservabilitySettings>()
    ?? new ObservabilitySettings();
var serviceName = ResolveServiceName(builder.Configuration, builder.Environment);
var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
var slowRequestThresholdMs = Math.Max(
    250,
    builder.Configuration.GetValue("Serilog:RequestLogging:SlowRequestThresholdMs", 2000));

builder.Logging.ClearProviders();
builder.Host.UseSerilog((context, _, loggerConfiguration) =>
    ConfigureSerilog(loggerConfiguration, context.Configuration, context.HostingEnvironment));
// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        if (context.HttpContext.Items.TryGetValue(RequestContextEnrichmentMiddleware.CorrelationIdItemName, out var correlationId))
        {
            context.ProblemDetails.Extensions["correlationId"] = correlationId;
        }
    };
});
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var secretProvider = serviceProvider.GetRequiredService<ISecretProvider>();
    options.UseNpgsql(
        ResolveConnectionString(secretProvider),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
            npgsqlOptions.CommandTimeout(15);
        });
});
builder.Services.Configure<SecretsSettings>(builder.Configuration.GetSection(SecretsSettings.SectionName));
builder.Services.Configure<ObservabilitySettings>(builder.Configuration.GetSection(ObservabilitySettings.SectionName));
builder.Services.AddSingleton<ISecretProvider>(CreateSecretProvider);
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection(JwtSettings.SectionName))
    .PostConfigure<ISecretProvider>((settings, secretProvider) =>
    {
        var secretKey = ResolveSecretValue(secretProvider, SecretNames.JwtSecretKey);
        if (!string.IsNullOrWhiteSpace(secretKey))
        {
            settings.SecretKey = secretKey;
        }
    });
builder.Services.AddOptions<BootstrapAdminOptions>()
    .Bind(builder.Configuration.GetSection(BootstrapAdminOptions.SectionName))
    .PostConfigure<ISecretProvider>((settings, secretProvider) =>
    {
        var password = ResolveSecretValue(secretProvider, SecretNames.BootstrapAdminPassword);
        if (!string.IsNullOrWhiteSpace(password))
        {
            settings.Password = password;
        }
    });
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection(CorsSettings.SectionName));
builder.Services.Configure<RateLimitingSettings>(builder.Configuration.GetSection(RateLimitingSettings.SectionName));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IQrExportService, QrExportService>();
builder.Services.AddApplicationServices();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is running."), tags: ["live"])
    .AddCheck<DatabaseReadinessHealthCheck>("database", tags: ["ready"]);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = WriteRateLimitResponseAsync;
    options.AddPolicy(
        RateLimitPolicyNames.Login,
        httpContext => CreateRateLimitPartition(httpContext, rateLimitingSettings.Login));
    options.AddPolicy(
        RateLimitPolicyNames.Refresh,
        httpContext => CreateRateLimitPartition(httpContext, rateLimitingSettings.Refresh));
});
builder.Services.AddSingleton<QueuedAuditLogSink>();
builder.Services.AddSingleton<IAuditLogSink>(serviceProvider => serviceProvider.GetRequiredService<QueuedAuditLogSink>());
builder.Services.AddHostedService<AuditLogBackgroundService>();
if (observabilitySettings.EnableOpenTelemetry)
{
    ConfigureOpenTelemetry(
        builder.Services,
        observabilitySettings,
        serviceName,
        serviceVersion,
        builder.Environment.EnvironmentName);
}
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>((options, jwtOptions) =>
    {
        var jwtSettings = jwtOptions.Value;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    AddAdminRolePolicy(options, AuthorizationPolicies.RequireRoleManagement, RoleCodes.SuperAdmin, RoleCodes.Support);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireAdminRead, RoleCodes.SuperAdmin, RoleCodes.Admin);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireAdminManagement, RoleCodes.SuperAdmin);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireAuditAccess, RoleCodes.SuperAdmin, RoleCodes.Admin);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireFinancialRead, RoleCodes.SuperAdmin, RoleCodes.Admin, RoleCodes.Finance);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireFinancialWrite, RoleCodes.SuperAdmin, RoleCodes.Finance);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireFinancialDelete, RoleCodes.SuperAdmin);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireLibraryRead, RoleCodes.SuperAdmin, RoleCodes.Admin, RoleCodes.Finance, RoleCodes.Support, RoleCodes.Viewer);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireLibraryWrite, RoleCodes.SuperAdmin, RoleCodes.Admin);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireLibraryDelete, RoleCodes.SuperAdmin);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireLibraryAccountCreate, RoleCodes.SuperAdmin, RoleCodes.Admin, RoleCodes.Support);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireLibraryAccountManagement, RoleCodes.SuperAdmin, RoleCodes.Admin, RoleCodes.Support);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequirePosRead, RoleCodes.SuperAdmin, RoleCodes.Admin, RoleCodes.Support);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequirePosWrite, RoleCodes.SuperAdmin, RoleCodes.Admin, RoleCodes.Support);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireQrRead, RoleCodes.SuperAdmin, RoleCodes.Admin, RoleCodes.Finance);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequireQrWrite, RoleCodes.SuperAdmin, RoleCodes.Admin);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequirePackageRead, RoleCodes.SuperAdmin, RoleCodes.Admin, RoleCodes.Viewer);
    AddAdminRolePolicy(options, AuthorizationPolicies.RequirePackageWrite, RoleCodes.SuperAdmin, RoleCodes.Admin);
    options.AddPolicy(AuthorizationPolicies.RequireLibraryAccount, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AuthorizationPolicies.UserTypeClaim, AuthorizationPolicies.LibraryAccountUserType);
    });
});
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "LibraryAPI",
        Version = "v1",
        Description = "Compatibility definition for existing Swagger clients."
    });
    options.SwaggerDoc("LibraryAPI", new OpenApiInfo
    {
        Title = "LibraryAPI",
        Version = "v1",
        Description = "Library operations, accounts, POS devices, and related endpoints."
    });
    options.SwaggerDoc("AdminAPI", new OpenApiInfo
    {
        Title = "AdminAPI",
        Version = "v1",
        Description = "Administration, authentication, roles, and admin user endpoints."
    });
    options.DocInclusionPredicate((documentName, apiDescription) =>
    {
        var groupName = apiDescription.GroupName ?? "LibraryAPI";
        if (string.Equals(documentName, "v1", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(groupName, "LibraryAPI", StringComparison.OrdinalIgnoreCase);
        }

        return string.Equals(groupName, documentName, StringComparison.OrdinalIgnoreCase);
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token only"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        ConfigureCorsPolicy(policy, corsSettings, builder.Environment);
    });
});


var app = builder.Build();
var secretProvider = app.Services.GetRequiredService<ISecretProvider>();
ValidateConnectionString(ResolveConnectionString(secretProvider));
ValidateJwtSettings(app.Services.GetRequiredService<IOptions<JwtSettings>>().Value, app.Environment);
app.Logger.LogInformation("Secret provider configured: {SecretProviderType}", secretProvider.ProviderType);
await ApplyDatabaseMigrationsAsync(app);
await NormalizeRoleCatalogAsync(app);
await EnsureBootstrapAdminAsync(app);

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();
app.UseMiddleware<RequestContextEnrichmentMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex is not null && ApiExceptionClassifier.IsRequestAborted(ex, httpContext))
            return LogEventLevel.Debug;

        if (ex != null)
            return LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= 400)
            return LogEventLevel.Warning;

        if (elapsed > slowRequestThresholdMs)
            return LogEventLevel.Warning;

        return LogEventLevel.Information;
    };

    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
        diagnosticContext.Set(
            "CorrelationId",
            httpContext.Items.TryGetValue(RequestContextEnrichmentMiddleware.CorrelationIdItemName, out var correlationId)
                ? correlationId
                : httpContext.TraceIdentifier);
        diagnosticContext.Set("IpAddress", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagnosticContext.Set("Endpoint", httpContext.Request.Path);
    };
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowFrontend");
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
                context.Response.Headers.Pragma = "no-cache";
                context.Response.Headers.Expires = "0";
                return Task.CompletedTask;
            });
        }

        await next();
    });

    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "LibraryAPI";
        options.SwaggerEndpoint("/swagger/LibraryAPI/swagger.json", "LibraryAPI v1");
        options.SwaggerEndpoint("/swagger/AdminAPI/swagger.json", "AdminAPI v1");
    });
}
app.UseAuthentication();
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase),
    apiApp =>
    {
        apiApp.UseMiddleware<AuditLoggingMiddleware>();
        apiApp.UseExceptionHandler();
    });
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live"),
    ResponseWriter = WriteHealthResponseAsync
})
    .AllowAnonymous();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponseAsync,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
})
    .AllowAnonymous();

app.MapControllers();

static async Task ApplyDatabaseMigrationsAsync(WebApplication app)
{
    if (!app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", false))
    {
        return;
    }

    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    for (var attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully.");
            return;
        }
        catch (Exception ex) when (attempt < 10)
        {
            logger.LogWarning(
                ex,
                "Database migration attempt {Attempt} failed. Retrying in 3 seconds.",
                attempt);

            await Task.Delay(TimeSpan.FromSeconds(3));
        }
    }

    await dbContext.Database.MigrateAsync();
}

static void ApplyEnvironmentOverrides(ConfigurationManager configuration)
{
    var overrides = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    AddEnvironmentOverride(overrides, "Jwt:Issuer", "MYAPI_JWT_ISSUER");
    AddEnvironmentOverride(overrides, "Jwt:Audience", "MYAPI_JWT_AUDIENCE");
    AddEnvironmentOverride(overrides, "Jwt:ExpiryMinutes", "MYAPI_JWT_EXPIRY_MINUTES");
    AddEnvironmentOverride(overrides, "BootstrapAdmin:Enabled", "MYAPI_BOOTSTRAP_ADMIN_ENABLED");
    AddEnvironmentOverride(overrides, "BootstrapAdmin:FullName", "MYAPI_BOOTSTRAP_ADMIN_FULLNAME");
    AddEnvironmentOverride(overrides, "BootstrapAdmin:Username", "MYAPI_BOOTSTRAP_ADMIN_USERNAME");
    AddEnvironmentOverride(overrides, "BootstrapAdmin:Email", "MYAPI_BOOTSTRAP_ADMIN_EMAIL");
    AddEnvironmentOverride(overrides, "BootstrapAdmin:PhoneNumber", "MYAPI_BOOTSTRAP_ADMIN_PHONE");
    AddEnvironmentOverride(overrides, "BootstrapAdmin:RoleCode", "MYAPI_BOOTSTRAP_ADMIN_ROLE");
    AddEnvironmentOverride(overrides, "Serilog:SeqServerUrl", "SERILOG_SEQ_URL");
    AddEnvironmentOverride(overrides, "Observability:ServiceName", "OTEL_SERVICE_NAME");
    AddEnvironmentOverride(overrides, "Observability:OtlpEndpoint", "OTEL_EXPORTER_OTLP_ENDPOINT");

    if (overrides.Count > 0)
    {
        configuration.AddInMemoryCollection(overrides);
    }
}

static void AddEnvironmentOverride(
    IDictionary<string, string?> overrides,
    string configurationKey,
    string environmentVariableName)
{
    var value = Environment.GetEnvironmentVariable(environmentVariableName);
    if (!string.IsNullOrWhiteSpace(value))
    {
        overrides[configurationKey] = value;
    }
}

static ISecretProvider CreateSecretProvider(IServiceProvider serviceProvider)
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var secretsSettings = serviceProvider.GetRequiredService<IOptions<SecretsSettings>>().Value;
    var logger = serviceProvider.GetRequiredService<ILogger<PlaceholderKeyVaultSecretProvider>>();

    return secretsSettings.ProviderType.Trim().ToLowerInvariant() switch
    {
        "" or "environment" => new EnvironmentSecretProvider(configuration),
        "keyvault" or "azurekeyvault" or "placeholderkeyvault" or "mock" =>
            new PlaceholderKeyVaultSecretProvider(configuration, logger),
        _ => throw new InvalidOperationException(
            $"Unsupported secret provider type '{secretsSettings.ProviderType}'.")
    };
}

static string ResolveConnectionString(ISecretProvider secretProvider)
{
    return ResolveSecretValue(secretProvider, SecretNames.DefaultConnectionString);
}

static string ResolveSecretValue(ISecretProvider secretProvider, string secretName)
{
    return secretProvider
        .GetSecretAsync(secretName)
        .AsTask()
        .ConfigureAwait(false)
        .GetAwaiter()
        .GetResult()?
        .Trim() ?? string.Empty;
}

static void ValidateConnectionString(string connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection is required. Configure it via environment variables or secure local secrets.");
    }
}

static void ValidateJwtSettings(JwtSettings jwtSettings, IHostEnvironment environment)
{
    if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
    {
        throw new InvalidOperationException("Jwt:Issuer is required.");
    }

    if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
    {
        throw new InvalidOperationException("Jwt:Audience is required.");
    }

    if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
    {
        throw new InvalidOperationException("Jwt:SecretKey is required.");
    }

    var keySizeInBytes = Encoding.UTF8.GetByteCount(jwtSettings.SecretKey);
    if (keySizeInBytes < 32)
    {
        throw new InvalidOperationException(
            "Jwt:SecretKey must be at least 32 bytes for HS256.");
    }

    if (environment.IsProduction() && IsPlaceholderJwtSecret(jwtSettings.SecretKey))
    {
        throw new InvalidOperationException(
            "Jwt:SecretKey is using an insecure placeholder value. Configure a production secret via environment variables.");
    }
}

static bool IsPlaceholderJwtSecret(string secretKey)
{
    return secretKey.Contains("change_this", StringComparison.OrdinalIgnoreCase)
        || secretKey.Contains("super_secret_key", StringComparison.OrdinalIgnoreCase);
}

static void ConfigureOpenTelemetry(
    IServiceCollection services,
    ObservabilitySettings settings,
    string serviceName,
    string serviceVersion,
    string environmentName)
{
    services.AddOpenTelemetry()
        .ConfigureResource(resource => resource
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment.name"] = environmentName
            }))
        .WithTracing(tracing =>
        {
            tracing.AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.Filter = httpContext => !httpContext.Request.Path.StartsWithSegments("/health");
            });
            tracing.AddHttpClientInstrumentation(options => options.RecordException = true);
            tracing.AddSource("Npgsql");
            tracing.AddSource("Microsoft.EntityFrameworkCore");

            if (settings.EnableConsoleExporter)
            {
                tracing.AddConsoleExporter();
            }

            if (TryResolveOtlpEndpoint(settings.OtlpEndpoint, out var otlpEndpoint))
            {
                tracing.AddOtlpExporter(options =>
                {
                    options.Endpoint = otlpEndpoint;
                    options.Protocol = ResolveOpenTelemetryOtlpProtocol(otlpEndpoint);
                });
            }
        })
        .WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation();
            metrics.AddHttpClientInstrumentation();
            metrics.AddRuntimeInstrumentation();

            if (settings.EnableConsoleExporter)
            {
                metrics.AddConsoleExporter();
            }

            if (TryResolveOtlpEndpoint(settings.OtlpEndpoint, out var otlpEndpoint))
            {
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = otlpEndpoint;
                    options.Protocol = ResolveOpenTelemetryOtlpProtocol(otlpEndpoint);
                });
            }
        });
}

static void ConfigureSerilog(
    LoggerConfiguration loggerConfiguration,
    IConfiguration configuration,
    IHostEnvironment environment)
{
    var observabilitySettings = configuration.GetSection(ObservabilitySettings.SectionName).Get<ObservabilitySettings>()
        ?? new ObservabilitySettings();
    var serviceName = ResolveServiceName(configuration, environment);
    var serviceVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";

    loggerConfiguration
        .MinimumLevel.Is(GetConfiguredLogLevel(configuration, "Serilog:MinimumLevel:Default", LogEventLevel.Information))
        .MinimumLevel.Override("Microsoft", GetConfiguredLogLevel(configuration, "Serilog:MinimumLevel:Override:Microsoft", LogEventLevel.Warning))
        .MinimumLevel.Override("Microsoft.AspNetCore", GetConfiguredLogLevel(configuration, "Serilog:MinimumLevel:Override:Microsoft.AspNetCore", LogEventLevel.Warning))
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", GetConfiguredLogLevel(configuration, "Serilog:MinimumLevel:Override:Microsoft.EntityFrameworkCore", LogEventLevel.Warning))
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", GetConfiguredLogLevel(configuration, "Serilog:MinimumLevel:Override:Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning))
        .MinimumLevel.Override("System", GetConfiguredLogLevel(configuration, "Serilog:MinimumLevel:Override:System", LogEventLevel.Warning))
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProperty("Application", environment.ApplicationName)
        .Enrich.WithProperty("ServiceName", serviceName)
        .Enrich.WithProperty("ServiceVersion", serviceVersion)
        .Enrich.WithProperty("Environment", environment.EnvironmentName)
        .WriteTo.Console();

    var filePath = configuration["Serilog:FilePath"];
    var enableFileLogging = configuration.GetValue("Serilog:EnableFileLogging", false);
    if ((enableFileLogging || !string.IsNullOrWhiteSpace(filePath)) && !string.IsNullOrWhiteSpace(filePath))
    {
        loggerConfiguration.WriteTo.File(
            filePath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: Math.Max(1, configuration.GetValue("Serilog:RetainedFileCountLimit", 14)));
    }

    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    if (!string.IsNullOrWhiteSpace(seqServerUrl))
    {
        loggerConfiguration.WriteTo.Seq(seqServerUrl);
    }

    if (TryResolveOtlpEndpoint(observabilitySettings.OtlpEndpoint, out var otlpEndpoint))
    {
        loggerConfiguration.WriteTo.OpenTelemetry(options =>
        {
            options.Endpoint = otlpEndpoint.ToString();
            options.Protocol = ResolveSerilogOtlpProtocol(otlpEndpoint);
            options.ResourceAttributes = new Dictionary<string, object>
            {
                ["service.name"] = serviceName,
                ["service.version"] = serviceVersion,
                ["deployment.environment.name"] = environment.EnvironmentName
            };
            options.IncludedData =
                IncludedData.TraceIdField
                | IncludedData.SpanIdField
                | IncludedData.MessageTemplateTextAttribute
                | IncludedData.SpecRequiredResourceAttributes;
        });
    }
}

static LogEventLevel GetConfiguredLogLevel(
    IConfiguration configuration,
    string key,
    LogEventLevel fallback)
{
    var raw = configuration[key];
    return Enum.TryParse<LogEventLevel>(raw, ignoreCase: true, out var parsed)
        ? parsed
        : fallback;
}

static string ResolveServiceName(IConfiguration configuration, IHostEnvironment environment)
{
    var configured = configuration[$"{ObservabilitySettings.SectionName}:ServiceName"];
    return string.IsNullOrWhiteSpace(configured)
        ? environment.ApplicationName
        : configured.Trim();
}

static bool TryResolveOtlpEndpoint(string? rawEndpoint, out Uri endpoint)
{
    if (!string.IsNullOrWhiteSpace(rawEndpoint) && Uri.TryCreate(rawEndpoint.Trim(), UriKind.Absolute, out endpoint!))
    {
        return true;
    }

    endpoint = default!;
    return false;
}

static OtlpExportProtocol ResolveOpenTelemetryOtlpProtocol(Uri endpoint)
{
    return IsHttpProtobufEndpoint(endpoint)
        ? OtlpExportProtocol.HttpProtobuf
        : OtlpExportProtocol.Grpc;
}

static OtlpProtocol ResolveSerilogOtlpProtocol(Uri endpoint)
{
    return IsHttpProtobufEndpoint(endpoint)
        ? OtlpProtocol.HttpProtobuf
        : OtlpProtocol.Grpc;
}

static bool IsHttpProtobufEndpoint(Uri endpoint)
{
    return endpoint.Port == 4318
        || endpoint.AbsolutePath.Contains("/v1/", StringComparison.OrdinalIgnoreCase);
}

static void ConfigureCorsPolicy(
    CorsPolicyBuilder policy,
    CorsSettings corsSettings,
    IHostEnvironment environment)
{
    var configuredOrigins = corsSettings.AllowedOrigins
        .Concat(ParseDelimitedValues(Environment.GetEnvironmentVariable("MYAPI_CORS_ALLOWED_ORIGINS")))
        .Where(static origin => !string.IsNullOrWhiteSpace(origin))
        .Select(static origin => origin.Trim())
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    policy.AllowAnyHeader().AllowAnyMethod();

    if (environment.IsDevelopment() && corsSettings.AllowLocalhostOriginsInDevelopment)
    {
        policy.SetIsOriginAllowed(origin =>
            configuredOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase)
            || IsDevelopmentLocalhostOrigin(origin));
        return;
    }

    if (configuredOrigins.Length > 0)
    {
        policy.WithOrigins(configuredOrigins);
        return;
    }

    policy.SetIsOriginAllowed(static _ => false);
}

static IEnumerable<string> ParseDelimitedValues(string? raw)
{
    return string.IsNullOrWhiteSpace(raw)
        ? []
        : raw.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

static bool IsDevelopmentLocalhostOrigin(string origin)
{
    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
    {
        return false;
    }

    if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
        && !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    return string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
        || string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase)
        || string.Equals(uri.Host, "::1", StringComparison.OrdinalIgnoreCase);
}

static RateLimitPartition<string> CreateRateLimitPartition(
    HttpContext httpContext,
    FixedWindowRateLimitSettings settings)
{
    var partitionKey = $"{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}:{httpContext.Request.Path.Value?.ToLowerInvariant()}";

    return RateLimitPartition.GetFixedWindowLimiter(
        partitionKey,
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = Math.Max(1, settings.PermitLimit),
            Window = TimeSpan.FromSeconds(Math.Max(1, settings.WindowSeconds)),
            QueueLimit = Math.Max(0, settings.QueueLimit),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true
        });
}

static async ValueTask WriteRateLimitResponseAsync(OnRejectedContext context, CancellationToken cancellationToken)
{
    if (context.HttpContext.Response.HasStarted)
    {
        return;
    }

    TimeSpan? retryAfter = null;
    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfterValue))
    {
        retryAfter = retryAfterValue;
        context.HttpContext.Response.Headers.RetryAfter =
            Math.Max(1, (int)Math.Ceiling(retryAfterValue.TotalSeconds)).ToString();
    }

    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
    context.HttpContext.Response.ContentType = "application/problem+json";

    await context.HttpContext.Response.WriteAsJsonAsync(new
    {
        type = "https://httpstatuses.com/429",
        title = "Too many requests.",
        status = StatusCodes.Status429TooManyRequests,
        traceId = context.HttpContext.TraceIdentifier,
        correlationId = context.HttpContext.Items.TryGetValue(RequestContextEnrichmentMiddleware.CorrelationIdItemName, out var correlationId)
            ? correlationId
            : context.HttpContext.TraceIdentifier,
        retryAfterSeconds = retryAfter is null ? (int?)null : Math.Max(1, (int)Math.Ceiling(retryAfter.Value.TotalSeconds))
    }, cancellationToken: cancellationToken);
}

static Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var payload = new
    {
        status = report.Status.ToString().ToLowerInvariant(),
        traceId = context.TraceIdentifier,
        correlationId = context.Items.TryGetValue(RequestContextEnrichmentMiddleware.CorrelationIdItemName, out var correlationId)
            ? correlationId
            : context.TraceIdentifier,
        checks = report.Entries.ToDictionary(
            static entry => entry.Key,
            static entry => new
            {
                status = entry.Value.Status.ToString().ToLowerInvariant(),
                description = entry.Value.Description,
                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2)
            })
    };

    return context.Response.WriteAsJsonAsync(payload);
}

static void AddAdminRolePolicy(AuthorizationOptions options, string policyName, params string[] roles)
{
    options.AddPolicy(policyName, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AuthorizationPolicies.UserTypeClaim, AuthorizationPolicies.AdminUserType);
        policy.RequireRole(roles);
    });
}

static async Task NormalizeRoleCatalogAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("RoleCatalogBootstrap");
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var roles = await dbContext.Roles
        .OrderBy(x => x.Id)
        .ToListAsync();

    var catalogChanged = false;
    foreach (var requiredRole in RoleCatalog.RequiredRoles)
    {
        var existingRole = roles.FirstOrDefault(x =>
            string.Equals(x.Code, requiredRole.Code, StringComparison.OrdinalIgnoreCase));

        if (existingRole is null)
        {
            existingRole = new Role
            {
                Name = requiredRole.Name,
                Code = requiredRole.Code,
                GuardName = requiredRole.GuardName,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Roles.Add(existingRole);
            roles.Add(existingRole);
            catalogChanged = true;
            continue;
        }

        if (!string.Equals(existingRole.Name, requiredRole.Name, StringComparison.Ordinal))
        {
            existingRole.Name = requiredRole.Name;
            catalogChanged = true;
        }

        if (!string.Equals(existingRole.Code, requiredRole.Code, StringComparison.Ordinal))
        {
            existingRole.Code = requiredRole.Code;
            catalogChanged = true;
        }

        if (existingRole.GuardName != requiredRole.GuardName)
        {
            existingRole.GuardName = requiredRole.GuardName;
            catalogChanged = true;
        }
    }

    if (catalogChanged)
    {
        await dbContext.SaveChangesAsync();
    }

    var canonicalRoles = (await dbContext.Roles
            .AsNoTracking()
            .ToListAsync())
        .Where(x => RoleCatalog.IsCanonicalRoleCode(x.Code, x.GuardName))
        .ToDictionary(x => (x.GuardName, x.Code), x => x);

    var reassignedAdminUsers = 0;
    var adminUsers = await dbContext.AdminUsers
        .Include(x => x.Role)
        .ToListAsync();

    foreach (var adminUser in adminUsers)
    {
        var hadKnownRole = RoleCatalog.TryNormalizeRoleCode(adminUser.Role?.Code, GuardName.Admin, out var normalizedCode);
        if (!hadKnownRole)
        {
            logger.LogWarning(
                "Admin user {Username} had unsupported role code {RoleCode}; mapped to {NormalizedRoleCode}.",
                adminUser.Username,
                adminUser.Role?.Code,
                normalizedCode);
        }

        if (!canonicalRoles.TryGetValue((GuardName.Admin, normalizedCode), out var canonicalRole))
        {
            throw new InvalidOperationException($"Canonical admin role '{normalizedCode}' is missing.");
        }

        if (adminUser.RoleId == canonicalRole.Id)
        {
            continue;
        }

        adminUser.RoleId = canonicalRole.Id;
        reassignedAdminUsers++;
    }

    var reassignedLibraryAccounts = 0;
    var libraryAccounts = await dbContext.LibraryAccounts
        .Include(x => x.Role)
        .ToListAsync();

    foreach (var libraryAccount in libraryAccounts)
    {
        var hadKnownRole = RoleCatalog.TryNormalizeRoleCode(libraryAccount.Role?.Code, GuardName.Office, out var normalizedCode);
        if (!hadKnownRole)
        {
            logger.LogWarning(
                "Library account {Username} had unsupported role code {RoleCode}; mapped to {NormalizedRoleCode}.",
                libraryAccount.Username,
                libraryAccount.Role?.Code,
                normalizedCode);
        }

        if (!canonicalRoles.TryGetValue((GuardName.Office, normalizedCode), out var canonicalRole))
        {
            throw new InvalidOperationException($"Canonical office role '{normalizedCode}' is missing.");
        }

        if (libraryAccount.RoleId == canonicalRole.Id)
        {
            continue;
        }

        libraryAccount.RoleId = canonicalRole.Id;
        reassignedLibraryAccounts++;
    }

    if (reassignedAdminUsers > 0 || reassignedLibraryAccounts > 0)
    {
        await dbContext.SaveChangesAsync();
    }

    var referencedRoleIds = await dbContext.AdminUsers
        .Select(x => x.RoleId)
        .Concat(dbContext.LibraryAccounts.Select(x => x.RoleId))
        .Distinct()
        .ToListAsync();

    var referencedRoleSet = referencedRoleIds.ToHashSet();
    var canonicalRoleIdSet = canonicalRoles.Values.Select(x => x.Id).ToHashSet();

    var obsoleteRoles = (await dbContext.Roles
            .ToListAsync())
        .Where(x => !canonicalRoleIdSet.Contains(x.Id))
        .Where(x => !referencedRoleSet.Contains(x.Id))
        .ToList();

    if (obsoleteRoles.Count > 0)
    {
        dbContext.Roles.RemoveRange(obsoleteRoles);
        await dbContext.SaveChangesAsync();
    }

    logger.LogInformation(
        "Role catalog normalized. Reassigned {AdminUsers} admin users, {LibraryAccounts} library accounts, removed {ObsoleteRoles} obsolete roles.",
        reassignedAdminUsers,
        reassignedLibraryAccounts,
        obsoleteRoles.Count);
}

static async Task EnsureBootstrapAdminAsync(WebApplication app)
{
    var options = app.Services
        .GetRequiredService<IOptions<BootstrapAdminOptions>>()
        .Value;

    if (!options.Enabled)
    {
        return;
    }

    if (string.IsNullOrWhiteSpace(options.FullName)
        || string.IsNullOrWhiteSpace(options.Username)
        || string.IsNullOrWhiteSpace(options.Password))
    {
        throw new InvalidOperationException(
            "BootstrapAdmin is enabled, but FullName, Username, and Password are required.");
    }

    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("BootstrapAdmin");
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var configuredRoleCode = string.IsNullOrWhiteSpace(options.RoleCode) ? RoleCodes.SuperAdmin : options.RoleCode;
    var roleCode = RoleCatalog.NormalizeRoleCode(configuredRoleCode, GuardName.Admin);
    var role = await dbContext.Roles
        .AsNoTracking()
        .FirstOrDefaultAsync(
            x => x.GuardName == GuardName.Admin && x.Code == roleCode)
        ?? await dbContext.Roles
            .AsNoTracking()
            .Where(x => x.GuardName == GuardName.Admin)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

    if (role is null)
    {
        throw new InvalidOperationException("No admin role is available for bootstrap admin creation.");
    }

    var username = options.Username.Trim();
    var admin = await dbContext.AdminUsers.FirstOrDefaultAsync(x => x.Username == username);
    var isNewAdmin = admin is null;

    if (admin is null)
    {
        admin = new AdminUser
        {
            Username = username,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.AdminUsers.Add(admin);
    }

    admin.FullName = options.FullName.Trim();
    admin.Email = string.IsNullOrWhiteSpace(options.Email) ? null : options.Email.Trim();
    admin.PhoneNumber = string.IsNullOrWhiteSpace(options.PhoneNumber) ? null : options.PhoneNumber.Trim();
    admin.PasswordHash = options.Password;
    admin.RoleId = role.Id;
    admin.Status = RecordStatus.Active;
    admin.UpdatedAt = DateTime.UtcNow;

    await dbContext.SaveChangesAsync();

    if (isNewAdmin)
    {
        logger.LogInformation(
            "Bootstrap admin '{Username}' created with role '{RoleCode}'.",
            admin.Username,
            role.Code);
    }
    else
    {
        logger.LogInformation(
            "Bootstrap admin '{Username}' synchronized with role '{RoleCode}'.",
            admin.Username,
            role.Code);
    }
}

app.Run();

public partial class Program;
