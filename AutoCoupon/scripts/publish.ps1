# AutoCoupon — Publikimi për Chrome Web Store
# Hape në PowerShell dhe ekzekuto: .\publish.ps1

$src = "D:\ANDROID\opencode\AutoCoupon"
$out = "D:\ANDROID\opencode\AutoCoupon\dist"

if (Test-Path $out) { Remove-Item -Recurse -Force $out }
New-Item -ItemType Directory -Path $out -Force | Out-Null

$files = @(
    "manifest.json",
    "background.js",
    "storage.js",
    "content.js",
    "popup/popup.html",
    "popup/popup.css",
    "popup/popup.js",
    "options/options.html",
    "options/options.js"
)

foreach ($f in $files) {
    $srcPath = Join-Path $src $f
    $destPath = Join-Path $out $f
    $parent = Split-Path $destPath -Parent
    if (!(Test-Path $parent)) { New-Item -ItemType Directory -Path $parent -Force | Out-Null }
    Copy-Item -LiteralPath $srcPath -Destination $destPath
}

# Kopjo ikonat (nëse ekzistojnë)
foreach ($size in @(16, 48, 128)) {
    $iconPath = Join-Path $src "icons\icon${size}.png"
    if (Test-Path $iconPath) {
        Copy-Item -LiteralPath $iconPath -Destination (Join-Path $out "icons\icon${size}.png")
    }
}

# Krijo ZIP
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zipPath = "D:\ANDROID\opencode\AutoCoupon\AutoCoupon-v1.0.zip"
if (Test-Path $zipPath) { Remove-Item -LiteralPath $zipPath }
[System.IO.Compression.ZipFile]::CreateFromDirectory($out, $zipPath)

Write-Host "`n=== Gati! ===" -ForegroundColor Green
Write-Host "Dosja: $out" -ForegroundColor Cyan
Write-Host "ZIP:   $zipPath" -ForegroundColor Cyan
Write-Host "`nHapat e ardhshëm:" -ForegroundColor Yellow
Write-Host "1. Gjenero ikonat duke hapur icons/generate_icons.html në browser" -ForegroundColor White
Write-Host "2. Shkarko icon16.png, icon48.png, icon128.png dhe vendosi në dist/icons/" -ForegroundColor White
Write-Host "3. Krijo screenshot 1280x800 të popup-it" -ForegroundColor White
Write-Host "4. Ngarko AutoCoupon-v1.0.zip në Chrome Web Store Dashboard" -ForegroundColor White
Write-Host "   https://chrome.google.com/webstore/devconsole" -ForegroundColor Cyan
