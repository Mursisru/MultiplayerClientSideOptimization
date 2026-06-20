param(
    [string]$GameRoot = "C:\Program Files (x86)\Steam\steamapps\common\Nuclear Option",
    [string]$Configuration = "DEV_SDK",
    [switch]$SkipPatchTool
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$EngineRoot = Join-Path (Split-Path -Parent $RepoRoot) "NOLoader_Engine"

if (-not (Test-Path $EngineRoot)) {
    Write-Error "NOLoader_Engine not found at $EngineRoot"
}

if (Get-Process -Name "NuclearOption" -ErrorAction SilentlyContinue) {
    Write-Error "Close Nuclear Option before deploy (PatchTool needs Managed DLLs unlocked)."
}

$project = Join-Path $RepoRoot "NOLoader.MultiplayerClientSideOptimization.csproj"
dotnet build $project -c $Configuration --verbosity minimal
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$dll = Join-Path $RepoRoot "bin\$Configuration\net48\NOLoader.MultiplayerClientSideOptimization.dll"
$modConfigProject = Join-Path $EngineRoot "DEV.SDK\shared\NOLoader.ModConfig\NOLoader.ModConfig.csproj"
$modConfigDll = Join-Path $EngineRoot "DEV.SDK\shared\NOLoader.ModConfig\bin\$Configuration\net48\NOLoader.ModConfig.dll"
if (-not (Test-Path $modConfigDll)) {
    dotnet build $modConfigProject -c $Configuration --verbosity minimal
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$modRoot = Join-Path $GameRoot "NOLoader\mods\MpClientOpt"
New-Item -ItemType Directory -Path $modRoot -Force | Out-Null

Copy-Item -Force $dll (Join-Path $modRoot "NOLoader.MultiplayerClientSideOptimization.dll")
Copy-Item -Force $modConfigDll (Join-Path $modRoot "NOLoader.ModConfig.dll")
Copy-Item -Force (Join-Path $RepoRoot "mod.json") (Join-Path $modRoot "mod.json")

$iniDst = Join-Path $modRoot "mod.ini"
Copy-Item -Force (Join-Path $RepoRoot "mod.ini") $iniDst
Write-Host "Updated mod.ini at $iniDst"

& (Join-Path $EngineRoot "scripts\pack-mod-rdytu.ps1") -ModFolder $modRoot

if (-not $SkipPatchTool) {
    Write-Host "Applying mod IL patches via PatchTool..."
    dotnet run --project (Join-Path $EngineRoot "src\NOLoader.PatchTool\NOLoader.PatchTool.csproj") -c $Configuration -- $GameRoot
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host "MpClientOpt deployed to $modRoot"
Write-Host "Join a dedicated server mission; check NOLoader ring log for [MpOpt] lines."
