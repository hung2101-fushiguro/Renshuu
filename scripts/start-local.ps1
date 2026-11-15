param(
    [string]$Server = 'localhost',
    [string]$Database = 'TracNghiemWeb',
    [ValidateRange(1024, 65535)][int]$Port = 5080,
    [switch]$NoBuild,
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug',
    [string]$StoragePath,
    [switch]$TestMode
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskPreviousConnection = $env:ConnectionStrings__DefaultConnection
$taskPreviousEnvironment = $env:ASPNETCORE_ENVIRONMENT
$taskPreviousStorage = $env:Documents__StoragePath
$taskPreviousLimit = $env:Accounts__RateLimitPerMinute
try {
    if ($TestMode -and ($Database -cnotmatch '^TracNghiemWeb_AccessTest_[a-f0-9]{32}$' -or [string]::IsNullOrWhiteSpace($StoragePath))) { throw 'TestMode chỉ nhận database và thư mục tệp kiểm thử riêng.' }
    $taskConnection = [System.Data.Common.DbConnectionStringBuilder]::new()
    $taskConnection['Server'] = $Server
    $taskConnection['Database'] = $Database
    $taskConnection['Trusted_Connection'] = 'True'
    $taskConnection['TrustServerCertificate'] = 'True'
    $taskConnection['MultipleActiveResultSets'] = 'True'
    $env:ConnectionStrings__DefaultConnection = $taskConnection.ConnectionString
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    if ($StoragePath) { $env:Documents__StoragePath = $StoragePath }
    if ($TestMode) { $env:Accounts__RateLimitPerMinute = '120' }
    $taskArguments = @('run', '--project', (Join-Path $taskRoot 'TracNghiemWeb.csproj'), '--configuration', $Configuration, '--no-launch-profile', '--urls', "http://localhost:$Port")
    if ($NoBuild) { $taskArguments += '--no-build' }
    & dotnet @taskArguments
    if ($LASTEXITCODE -ne 0) { throw "Ứng dụng dừng với mã lỗi $LASTEXITCODE." }
}
finally {
    $env:ConnectionStrings__DefaultConnection = $taskPreviousConnection
    $env:ASPNETCORE_ENVIRONMENT = $taskPreviousEnvironment
    $env:Documents__StoragePath = $taskPreviousStorage
    $env:Accounts__RateLimitPerMinute = $taskPreviousLimit
}
