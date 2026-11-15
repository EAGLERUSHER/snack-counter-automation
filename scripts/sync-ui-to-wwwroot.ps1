$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$ui = Join-Path $root "client\snack-counter-ui"
$wwwroot = Join-Path $root "src\SnackCounter.Api\wwwroot"
$dist = Join-Path $ui "dist\snack-counter-ui\browser"

Push-Location $ui
npm run build
Pop-Location

if (-not (Test-Path $dist)) {
    throw "Angular build output not found at $dist"
}

if (Test-Path $wwwroot) { Remove-Item $wwwroot -Recurse -Force }
New-Item -ItemType Directory -Path $wwwroot | Out-Null
Copy-Item -Path (Join-Path $dist "*") -Destination $wwwroot -Recurse
Write-Host "Copied UI to $wwwroot"
Write-Host "Run: dotnet run --project src/SnackCounter.Api --urls http://localhost:5018"