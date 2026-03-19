[CmdletBinding()]
param(
    [string]$FileName = ("myapi-backup-{0}.sql" -f (Get-Date -Format "yyyyMMdd-HHmmss")),
    [int]$MaxAttempts = 20,
    [int]$DelaySeconds = 3
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$backupDirectory = Join-Path $repoRoot "docker\\backups"
$hostBackupPath = Join-Path $backupDirectory $FileName
$containerBackupPath = "/backups/$FileName"
$composeEnvPath = Join-Path $repoRoot ".env"

$composeEnv = @{}
if (Test-Path $composeEnvPath)
{
    foreach ($line in Get-Content -Path $composeEnvPath)
    {
        if ([string]::IsNullOrWhiteSpace($line) -or $line.TrimStart().StartsWith("#"))
        {
            continue
        }

        $separatorIndex = $line.IndexOf("=")
        if ($separatorIndex -lt 1)
        {
            continue
        }

        $name = $line.Substring(0, $separatorIndex).Trim()
        $value = $line.Substring($separatorIndex + 1).Trim()
        $composeEnv[$name] = $value
    }
}

function Get-ComposeVariable
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [string]$DefaultValue = ""
    )

    $environmentValue = [Environment]::GetEnvironmentVariable($Name)
    if (-not [string]::IsNullOrWhiteSpace($environmentValue))
    {
        return $environmentValue.Trim()
    }

    if ($composeEnv.ContainsKey($Name) -and -not [string]::IsNullOrWhiteSpace($composeEnv[$Name]))
    {
        return $composeEnv[$Name].Trim()
    }

    return $DefaultValue
}

$postgresUser = Get-ComposeVariable -Name "POSTGRES_USER" -DefaultValue "myapi"
$postgresDatabase = Get-ComposeVariable -Name "POSTGRES_DB" -DefaultValue "myapi"

if (Test-Path $hostBackupPath)
{
    throw "Backup file already exists: $hostBackupPath"
}

New-Item -ItemType Directory -Path $backupDirectory -Force | Out-Null

Push-Location $repoRoot
try
{
    & docker compose up -d db api
    if ($LASTEXITCODE -ne 0)
    {
        throw "Unable to start Docker services."
    }

    $superAdminCount = 0
    $superAdminCheckQuery = "SELECT COUNT(*) FROM admin_users au INNER JOIN roles r ON r.id = au.role_id WHERE r.code = 'super_admin';"
    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++)
    {
        $superAdminCountRaw = & docker compose exec -T db psql -U $postgresUser -d $postgresDatabase -tAc $superAdminCheckQuery
        if ($LASTEXITCODE -eq 0 -and [int]::TryParse(($superAdminCountRaw | Out-String).Trim(), [ref]$superAdminCount) -and $superAdminCount -gt 0)
        {
            break
        }

        if ($attempt -eq $MaxAttempts)
        {
            throw "No super admin data was found in the database after waiting for bootstrap. Export stopped."
        }

        Start-Sleep -Seconds $DelaySeconds
    }

    & docker compose exec -T db pg_dump -U $postgresUser -d $postgresDatabase --clean --if-exists --no-owner --no-privileges -f $containerBackupPath
    if ($LASTEXITCODE -ne 0)
    {
        throw "Database export failed."
    }

    Write-Host "Database export completed successfully."
    Write-Host "Backup file: $hostBackupPath"
    Write-Host "Verified super_admin rows: $superAdminCount"
}
finally
{
    Pop-Location
}
