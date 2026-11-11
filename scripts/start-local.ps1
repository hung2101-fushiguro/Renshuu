param(
    [string]$Server = 'localhost',
    [string]$Database = 'RenshuuUiDev',
    [ValidateRange(1024, 65535)][int]$Port = 5186,
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$projectDirectory = Split-Path -Parent $PSScriptRoot
$previousConnection = $env:ConnectionStrings__DefaultConnection
$previousEnvironment = $env:ASPNETCORE_ENVIRONMENT
try {
    $env:ConnectionStrings__DefaultConnection = "Server=$Server;Database=$Database;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $runArguments = @('run', '--project', (Join-Path $projectDirectory 'TracNghiemWeb.csproj'), '--no-launch-profile', '--urls', "http://localhost:$Port")
    if ($NoBuild) { $runArguments += '--no-build' }
    & dotnet @runArguments
    if ($LASTEXITCODE -ne 0) { throw "Ứng dụng dừng với mã lỗi $LASTEXITCODE." }
}
finally {
    $env:ConnectionStrings__DefaultConnection = $previousConnection
    $env:ASPNETCORE_ENVIRONMENT = $previousEnvironment
}
