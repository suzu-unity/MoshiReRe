$ErrorActionPreference = "Stop"

$toolRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$comfyScript = Join-Path $toolRoot "Start-MossComfy.ps1"
$agentScript = Join-Path $toolRoot "Start-ComfyAgent.ps1"

try {
    Invoke-RestMethod "http://127.0.0.1:8188/system_stats" -TimeoutSec 2 | Out-Null
}
catch {
    Start-Process powershell.exe `
        -ArgumentList "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $comfyScript `
        -WindowStyle Hidden
}

$deadline = (Get-Date).AddSeconds(90)
do {
    try {
        Invoke-RestMethod "http://127.0.0.1:8188/system_stats" -TimeoutSec 2 | Out-Null
        break
    }
    catch {
        Start-Sleep -Milliseconds 500
    }
} while ((Get-Date) -lt $deadline)

if ((Get-Date) -ge $deadline) {
    throw "ComfyUI did not become ready at http://127.0.0.1:8188."
}

Start-Process powershell.exe `
    -ArgumentList "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", $agentScript `
    -WindowStyle Hidden

Start-Process "http://127.0.0.1:8188"
