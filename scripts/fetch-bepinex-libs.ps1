param(
    [string]$Version = "5.4.23.3"
)

$ErrorActionPreference = "Stop"
$libDir = Join-Path (Split-Path -Parent $PSScriptRoot) "BepInEx\lib"
New-Item -ItemType Directory -Path $libDir -Force | Out-Null

$zip = Join-Path $env:TEMP "BepInEx_unix_$Version.zip"
$url = "https://github.com/BepInEx/BepInEx/releases/download/v5.4.23/BepInEx_unix_$Version.zip"
Write-Host "Downloading $url"
Invoke-WebRequest -Uri $url -OutFile $zip

$extract = Join-Path $env:TEMP "bepinex_extract_$Version"
if (Test-Path $extract) { Remove-Item -Recurse -Force $extract }
Expand-Archive -Path $zip -DestinationPath $extract -Force

Copy-Item -Force (Join-Path $extract "BepInEx\core\BepInEx.dll") $libDir
Copy-Item -Force (Join-Path $extract "BepInEx\core\0Harmony.dll") $libDir
Write-Host "BepInEx build refs ready in $libDir"
