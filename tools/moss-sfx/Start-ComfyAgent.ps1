$ErrorActionPreference = "Stop"

$env:COMFYUI_URL = "http://127.0.0.1:8188"
$env:COMFYUI_PATH = "C:\Users\suzu\ComfyUI-MOSS"
$env:COMFYUI_PYTHON = "C:\Users\suzu\ComfyUI-MOSS\.venv\Scripts\python.exe"

Set-Location -LiteralPath "C:\Users\suzu\ComfyUI-MOSS"
& "C:\Program Files\nodejs\npx.cmd" -y comfyui-mcp@latest connect
