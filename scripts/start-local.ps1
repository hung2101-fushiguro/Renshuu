param(
    [string]$Server = 'localhost',
    [string]$Database = 'TracNghiemWeb',
    [ValidateRange(1024, 65535)][int]$Port = 5080,
    [switch]$NoBuild
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskPreviousConnection = $env:ConnectionStrings__DefaultConnection
$taskPreviousEnvironment = $env:ASPNETCORE_ENVIRONMENT
try {
    $taskConnection = [System.Data.Common.DbConnectionStringBuilder]::new()
    $taskConnection['Server'] = $Server
    $taskConnection['Database'] = $Database
    $taskConnection['Trusted_Connection'] = 'True'
    $taskConnection['TrustServerCertificate'] = 'True'
    $taskConnection['MultipleActiveResultSets'] = 'True'
    $env:ConnectionStrings__DefaultConnection = $taskConnection.ConnectionString
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $taskArguments = @('run', '--project', (Join-Path $taskRoot 'TracNghiemWeb.csproj'), '--no-launch-profile', '--urls', "http://localhost:$Port")
    if ($NoBuild) { $taskArguments += '--no-build' }
    & dotnet @taskArguments
    if ($LASTEXITCODE -ne 0) { throw "Ứng dụng dừng với mã lỗi $LASTEXITCODE." }
}
finally {
    $env:ConnectionStrings__DefaultConnection = $taskPreviousConnection
    $env:ASPNETCORE_ENVIRONMENT = $taskPreviousEnvironment
}
