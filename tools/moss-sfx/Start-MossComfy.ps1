$ErrorActionPreference = "Stop"

$comfyRoot = "C:\Users\suzu\ComfyUI-MOSS"
$pythonPath = Join-Path $comfyRoot ".venv\Scripts\python.exe"
$outputPath = "D:\Unity\MoshiReRe\Assets\Audio\SFX\Generated"

if (-not (Test-Path -LiteralPath $pythonPath)) {
    throw "MOSS ComfyUI environment is not installed: $pythonPath"
}

New-Item -ItemType Directory -Force -Path $outputPath | Out-Null
Set-Location -LiteralPath $comfyRoot

# Ignore a stale global Hugging Face token for this public model without
# changing or deleting the user's existing Hugging Face login.
$env:HF_HUB_DISABLE_IMPLICIT_TOKEN = "1"

& $pythonPath main.py `
    --listen 127.0.0.1 `
    --port 8188 `
    --output-directory $outputPath `
    --preview-method auto
