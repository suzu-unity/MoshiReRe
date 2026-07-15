param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Arguments
)

$ErrorActionPreference = "Stop"
python (Join-Path $PSScriptRoot "grok_agent.py") @Arguments
