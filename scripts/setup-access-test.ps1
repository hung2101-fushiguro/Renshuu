param([string]$Server = 'localhost', [ValidateRange(1024,65535)][int]$Port = 5081, [switch]$NoBuild)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskArtifact = Join-Path $taskRoot 'artifacts'
New-Item -ItemType Directory -Path $taskArtifact -Force | Out-Null
if (!$NoBuild) {
    & dotnet build (Join-Path $taskRoot 'TracNghiemWeb.csproj') -c Release --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
}
$taskId = [Guid]::NewGuid().ToString('N')
$taskDatabase = 'TracNghiemWeb_AccessTest_' + $taskId
$taskStorage = Join-Path ([IO.Path]::GetTempPath()) ('TracNghiemWeb_AccessFiles_' + $taskId)
$taskAccounts = @{}
foreach ($taskRole in @('admin','userA','userB')) {
    $taskAccounts[$taskRole] = @{Email=($taskRole.ToLowerInvariant() + '@access.example.test'); Password=('Aa9!' + [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(24)))}
}
[pscustomobject]@{Database=$taskDatabase; Storage=$taskStorage; Port=$Port; Accounts=$taskAccounts} | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $taskArtifact 'access-test.json') -Encoding utf8
$taskNames = @('ASPNETCORE_ENVIRONMENT','ConnectionStrings__DefaultConnection','Documents__StoragePath','TRACNGHIEM_PROVISION_PASSWORD')
$taskPrevious = @{}
foreach ($taskName in $taskNames) { $taskPrevious[$taskName] = [Environment]::GetEnvironmentVariable($taskName, 'Process') }
try {
    $env:ASPNETCORE_ENVIRONMENT = 'Development'
    $env:ConnectionStrings__DefaultConnection = "Server=$Server;Database=$taskDatabase;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
    $env:Documents__StoragePath = $taskStorage
    $taskApp = Join-Path $taskRoot 'bin\Release\net8.0\TracNghiemWeb.dll'
    & dotnet $taskApp --migrate --seed-demo *> (Join-Path $taskArtifact 'access-setup.log')
    if ($LASTEXITCODE -ne 0) { throw 'Test migration failed; see artifacts/access-setup.log.' }
    foreach ($taskRole in @('admin','userA','userB')) {
        $env:TRACNGHIEM_PROVISION_PASSWORD = $taskAccounts[$taskRole].Password
        $taskAssignedRole = if ($taskRole -eq 'admin') {'Admin'} else {'User'}
        & dotnet $taskApp --provision-user $taskAccounts[$taskRole].Email --role $taskAssignedRole --name $taskRole *>> (Join-Path $taskArtifact 'access-setup.log')
        if ($LASTEXITCODE -ne 0) { throw 'Test account setup failed; see artifacts/access-setup.log.' }
    }
    Write-Output "Created disposable database $taskDatabase. Fixture: artifacts/access-test.json"
}
finally { foreach ($taskName in $taskNames) { [Environment]::SetEnvironmentVariable($taskName, $taskPrevious[$taskName], 'Process') } }
