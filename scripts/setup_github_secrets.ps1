# PowerShell script to encode YouTube credentials for GitHub Secrets
# Run this on your local machine to get the values to add to GitHub

$ErrorActionPreference = "Stop"
$projectDir = Split-Path -Parent (Split-Path -Parent $PSCommandPath)

$clientSecretPath = "$projectDir\client_secret.json"
$tokenPath = "$projectDir\token.pickle"

Write-Host "=== GitHub Secrets Setup ===" -ForegroundColor Cyan
Write-Host ""

# Encode client_secret.json
if (Test-Path $clientSecretPath) {
    $content = Get-Content $clientSecretPath -Raw
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($content)
    $encoded = [Convert]::ToBase64String($bytes)
    Write-Host "CLIENT_SECRET_JSON (add this to GitHub Secrets):" -ForegroundColor Yellow
    Write-Host $encoded
    Write-Host ""
} else {
    Write-Host "[X] client_secret.json not found at: $clientSecretPath" -ForegroundColor Red
    Write-Host "    Download it from Google Cloud Console > APIs & Services > Credentials" -ForegroundColor Yellow
}

# Encode token.pickle
if (Test-Path $tokenPath) {
    $bytes = [System.IO.File]::ReadAllBytes($tokenPath)
    $encoded = [Convert]::ToBase64String($bytes)
    Write-Host "TOKEN_PICKLE (add this to GitHub Secrets):" -ForegroundColor Yellow
    Write-Host $encoded
    Write-Host ""
} else {
    Write-Host "[i] token.pickle not found. Run youtube_oauth.py first to generate it." -ForegroundColor Yellow
    Write-Host "    Or we can generate it during the first GitHub Actions run." -ForegroundColor Yellow
}

Write-Host "=== How to add to GitHub ===" -ForegroundColor Cyan
Write-Host "1. Go to: https://github.com/resulmilaimi91-web/NaturalVitalityHub/settings/secrets/actions"
Write-Host "2. Click 'New repository secret'"
Write-Host "3. Add CLIENT_SECRET_JSON with the value above"
Write-Host "4. Add TOKEN_PICKLE with the value above"
Write-Host "5. Click 'New repository secret' again if adding both"
Write-Host ""
Write-Host "Also add your Digistore24 PayPal email:" -ForegroundColor Cyan
Write-Host "6. Add PAYPAL_EMAIL = resul.paypal@gmail.com"
