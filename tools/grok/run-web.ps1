param(
    [int]$Port = 8787
)

$ErrorActionPreference = "Stop"
python (Join-Path $PSScriptRoot "web_server.py") --port $Port
