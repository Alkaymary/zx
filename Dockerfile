FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["MyApi.csproj", "./"]
RUN dotnet restore "MyApi.csproj"

COPY . .
RUN dotnet publish "MyApi.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && adduser --disabled-password --gecos "" --home /app appuser \
    && mkdir -p /app/logs \
    && chown -R appuser:appuser /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

COPY --from=build --chown=appuser:appuser /app/publish .

USER appuser

EXPOSE 8080

HEALTHCHECK --interval=15s --timeout=5s --start-period=20s --retries=10 \
    CMD curl --fail --silent http://localhost:8080/health/ready || exit 1

ENTRYPOINT ["dotnet", "MyApi.dll"]
