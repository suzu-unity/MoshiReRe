[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$music3Root = "G:\ComfyUI_Flux_Workspace\ComfyUI_Music3"
$pythonPath = "G:\ComfyUI_flux_workspace\ComfyUI_windows_portable\python_embeded\python.exe"
$music3Url = "http://127.0.0.1:8190"
$agentBridgePort = 9181

try {
    Invoke-RestMethod -Uri "$music3Url/system_stats" -TimeoutSec 2 | Out-Null
}
catch {
    throw "Music3 is not ready at $music3Url. Run Start-Music3Environment.ps1 first."
}

$existingBridge = Get-NetTCPConnection -LocalPort $agentBridgePort -State Listen -ErrorAction SilentlyContinue
if ($existingBridge) {
    Write-Output "A Comfy Agent bridge is already listening on port $agentBridgePort; it was left untouched."
    return
}

$npxPath = (Get-Command npx.cmd -ErrorAction Stop).Source
if (-not (Test-Path -LiteralPath $pythonPath)) {
    throw "Music3 portable Python was not found: $pythonPath"
}
$env:COMFYUI_URL = $music3Url
$env:COMFYUI_PATH = $music3Root
$env:COMFYUI_PYTHON = $pythonPath
$env:COMFYUI_MCP_BRIDGE_PORT = "$agentBridgePort"

Set-Location -LiteralPath $music3Root
& $npxPath -y comfyui-mcp@latest connect $music3Url
