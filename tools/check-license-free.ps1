param([string]$RepositoryRoot = (Split-Path $PSScriptRoot -Parent))

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'LicenseFreeChecks/LicenseFreeChecks.csproj'
dotnet restore $project --locked-mode
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet run --project $project --configuration Release --no-restore -- $RepositoryRoot
exit $LASTEXITCODE
