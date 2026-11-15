param([string]$Server = 'localhost')
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path -Parent $PSScriptRoot
$taskSession = Get-Content (Join-Path $taskRoot 'artifacts\access-test.json') -Raw | ConvertFrom-Json
$taskMatch = [regex]::Match($taskSession.Database, '^TracNghiemWeb_AccessTest_([a-f0-9]{32})$')
if (!$taskMatch.Success) { throw 'Not a disposable access-test database.' }
$taskStorage = [IO.Path]::GetFullPath($taskSession.Storage)
$taskTemp = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar)
if (![string]::Equals([IO.Path]::GetDirectoryName($taskStorage), $taskTemp, [StringComparison]::OrdinalIgnoreCase) -or [IO.Path]::GetFileName($taskStorage) -ne ('TracNghiemWeb_AccessFiles_' + $taskMatch.Groups[1].Value)) { throw 'Unexpected storage path; refusing cleanup.' }
$taskDatabase = $taskSession.Database
& sqlcmd -S $Server -d master -E -C -b -Q "IF DB_ID(N'$taskDatabase') IS NOT NULL DROP DATABASE [$taskDatabase];"
if ($LASTEXITCODE -ne 0) { throw 'Stop the test host before cleaning up. No files were removed.' }
if (Test-Path -LiteralPath $taskStorage) { Remove-Item -LiteralPath $taskStorage -Recurse -Force }
Write-Output "Removed disposable database $taskDatabase and its test files."
