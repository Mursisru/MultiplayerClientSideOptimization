param(
    [string]$GameRoot = "C:\Program Files (x86)\Steam\steamapps\common\Nuclear Option",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $RepoRoot "BepInEx\MultiplayerClientSideOptimization\MultiplayerClientSideOptimization.BepInEx.csproj"

dotnet build $project -c $Configuration --verbosity minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$dll = Join-Path $RepoRoot "BepInEx\MultiplayerClientSideOptimization\bin\$Configuration\MultiplayerClientSideOptimization.dll"
$pluginRoot = Join-Path $GameRoot "BepInEx\plugins\$Configuration\com.at747.multiplayerclientsideoptimization"
New-Item -ItemType Directory -Path $pluginRoot -Force | Out-Null

Copy-Item -Force $dll (Join-Path $pluginRoot "MultiplayerClientSideOptimization.dll")
$iniDst = Join-Path $pluginRoot "mod.ini"
if (-not (Test-Path $iniDst)) {
    Copy-Item -Force (Join-Path $RepoRoot "mod.ini") $iniDst
}

Write-Host "BepInEx plugin deployed to $pluginRoot"
