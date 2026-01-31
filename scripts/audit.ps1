param (
    [Parameter(Mandatory=$true)]
    [ValidateSet("frontend", "backend", "security")]
    [string]$Type,

    [Parameter(Mandatory=$true)]
    [string]$FilePath
)

if (-not (Test-Path $FilePath)) {
    Write-Error "File not found: $FilePath"
    exit 1
}

$TemplatePath = ".gemini/code-review-$Type.md"
if ($Type -eq "security") { $TemplatePath = ".gemini/security-audit.md" }

$Prompt = Get-Content $TemplatePath -Raw
$Code = Get-Content $FilePath -Raw

Write-Host "--- Running $Type Audit on $FilePath ---" -ForegroundColor Cyan

# This command prepares the prompt for you to copy-paste or pipe into gemini-cli
$FinalPrompt = "$Prompt `n`n FILE CONTENT: `n $Code"

# If you want to run it automatically (assuming gemini command is available):
# gemini -p $FinalPrompt

$FinalPrompt | Set-Clipboard
Write-Host "Prompt copied to clipboard! You can now paste it into your gemini-cli session." -ForegroundColor Green
