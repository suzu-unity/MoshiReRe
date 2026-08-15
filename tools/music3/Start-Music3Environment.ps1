[CmdletBinding()]
param(
    [ValidateRange(5, 600)]
    [int]$StartupTimeoutSeconds = 90,
    [switch]$NoAgent,
    [switch]$ValidateOnly
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$music3Root = "G:\ComfyUI_Flux_Workspace\ComfyUI_Music3"
$launchScript = Join-Path $music3Root "launch_music3.py"
$templatePath = Join-Path $music3Root "python_overlay\comfyui_workflow_templates_json\templates\audio_minimax_music_3.json"
$modelConfigPath = Join-Path $music3Root "extra_model_paths.yaml"
$sharedComfyRoot = "G:\ComfyUI_flux_workspace\ComfyUI_windows_portable\ComfyUI"
$pythonPath = "G:\ComfyUI_flux_workspace\ComfyUI_windows_portable\python_embeded\python.exe"
$sharedModelsRoot = Join-Path $sharedComfyRoot "models"
$outputPath = "D:\Unity\MoshiReRe\Assets\Audio\BGM\Generated"
$music3Port = 8190
$music3Url = "http://127.0.0.1:$music3Port"
$agentScript = Join-Path $PSScriptRoot "Start-Music3ComfyAgent.ps1"
$agentBridgePort = 9181

function Test-Music3Ready {
    try {
        Invoke-RestMethod -Uri "$music3Url/system_stats" -TimeoutSec 2 | Out-Null
        $nodeInfo = Invoke-RestMethod -Uri "$music3Url/object_info/MiniMaxMusic3TextEncode" -TimeoutSec 3
        return $null -ne $nodeInfo.MiniMaxMusic3TextEncode
    }
    catch {
        return $false
    }
}

function Get-ListeningProcessIds {
    param([int]$Port)

    $listeners = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
    if (-not $listeners) {
        return @()
    }

    return @($listeners | Select-Object -ExpandProperty OwningProcess -Unique)
}

foreach ($requiredPath in @($music3Root, $launchScript, $templatePath, $modelConfigPath, $pythonPath, $agentScript)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Music3 prerequisite is missing: $requiredPath"
    }
}

$requiredModels = @(
    (Join-Path $sharedModelsRoot "diffusion_models\minimax_music3_dit_int8_convrot.safetensors"),
    (Join-Path $sharedModelsRoot "text_encoders\minimax_music3_text_encoder_pruned_int8_convrot.safetensors"),
    (Join-Path $sharedModelsRoot "vae\minimax_music3_dav.safetensors")
)
$missingModels = @($requiredModels | Where-Object { -not (Test-Path -LiteralPath $_) })
if ($missingModels.Count -gt 0) {
    throw "Music3 model files are missing: $($missingModels -join '; ')"
}

$configuredModelRoot = ((Get-Content -LiteralPath $modelConfigPath -Raw).Replace('/', '\')).ToLowerInvariant()
$expectedModelRoot = $sharedComfyRoot.ToLowerInvariant()
if (-not $configuredModelRoot.Contains($expectedModelRoot)) {
    throw "extra_model_paths.yaml does not point at the shared Music3 model root: $sharedModelsRoot"
}

$serverReady = Test-Music3Ready
if ($ValidateOnly) {
    $state = if ($serverReady) { "ready" } else { "not running" }
    Write-Output "Music3 prerequisites are valid; server state: $state; endpoint: $music3Url"
    return
}

New-Item -ItemType Directory -Force -Path $outputPath | Out-Null

if (-not $serverReady) {
    $blockingProcessIds = Get-ListeningProcessIds -Port $music3Port
    if ($blockingProcessIds.Count -gt 0) {
        throw "Port $music3Port is in use by PID(s) $($blockingProcessIds -join ', ') but is not a healthy Music3 ComfyUI server. No process was stopped."
    }

    $launchArguments = @(
        ('"{0}"' -f $launchScript),
        "--listen",
        "127.0.0.1",
        "--port",
        "$music3Port",
        "--output-directory",
        ('"{0}"' -f $outputPath)
    ) -join " "

    Start-Process -FilePath $pythonPath -ArgumentList $launchArguments -WorkingDirectory $music3Root -WindowStyle Hidden | Out-Null

    $deadline = (Get-Date).AddSeconds($StartupTimeoutSeconds)
    do {
        Start-Sleep -Milliseconds 500
        $serverReady = Test-Music3Ready
    } while (-not $serverReady -and (Get-Date) -lt $deadline)

    if (-not $serverReady) {
        throw "Music3 did not become ready at $music3Url within $StartupTimeoutSeconds seconds. The started process was left running for inspection."
    }
}

if (-not $NoAgent) {
    $agentProcessIds = Get-ListeningProcessIds -Port $agentBridgePort
    if ($agentProcessIds.Count -eq 0) {
        $agentArguments = "-NoProfile -ExecutionPolicy Bypass -File `"$agentScript`""
        Start-Process -FilePath "powershell.exe" -ArgumentList $agentArguments -WindowStyle Hidden | Out-Null
    }
}

Write-Output "Music3 is ready at $music3Url. BGM output: $outputPath. Open the URL and use the Agent panel Connect action when needed."
