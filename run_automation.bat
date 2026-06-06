@echo off
title NaturalVitalityHub - YouTube Automation
cd /d "%~dp0"

:menu
cls
echo =============================================
echo  NaturalVitalityHub - YouTube Automation
echo  Channel: @NaturalVitalityHub-y4d
echo =============================================
echo.
echo  1. Generate ALL video packages
echo  2. List all Digistore24 products
echo  3. Generate single product video
echo  4. Start 24/7 Scheduler
echo  5. Export products to CSV
echo  6. Show Dashboard
echo  7. Create video from saved script
echo  8. Exit
echo.
set /p choice="Select option (1-8): "

if "%choice%"=="1" python youtube_content_automation.py all
if "%choice%"=="2" python youtube_content_automation.py list
if "%choice%"=="3" python youtube_content_automation.py generate
if "%choice%"=="4" python youtube_content_automation.py schedule
if "%choice%"=="5" python youtube_content_automation.py export
if "%choice%"=="6" python youtube_content_automation.py dashboard
if "%choice%"=="7" goto create_video
if "%choice%"=="8" exit /b

echo.
pause
goto menu

:create_video
cls
echo =============================================
echo  Create Video from Saved Script
echo =============================================
echo.
echo  Script folders:
dir /b /ad "output\scripts\" 2>nul
if errorlevel 1 echo  No scripts found. Run option 1 first.
echo.
set /p folder="Enter folder name: "
if "%folder%"=="" goto menu
python -c "
import sys, json, os
sys.path.insert(0, '.')
from youtube_content_automation import *
from pathlib import Path
folder = Path('output/scripts/%folder%')
pkg_file = folder / 'package.json'
if pkg_file.exists():
    with open(pkg_file) as f:
        meta = json.load(f)
    with open(folder / 'script.txt') as f:
        script = f.read()
    with open(folder / 'description.txt') as f:
        desc = f.read()
    pkg = {
        'title': meta['title'],
        'product': meta['product'],
        'description': desc,
        'script': script,
        'tags': ['health', 'supplements', meta['product'].replace(' ', '')],
        'affiliate_url': meta.get('affiliate_url', ''),
        'niche': meta.get('niche', 'general')
    }
    vp = create_video_from_package(pkg, meta['product'][:20])
    if vp:
        print(f'\nVideo created: {vp}')
        up = input('Upload to YouTube? (yes/no): ')
        if up.lower() in ('yes','y','po'):
            from youtube_upload import upload_video
            url = upload_video(vp, pkg['title'], pkg['description'], pkg['tags'], 'public')
            print(f'Uploaded: {url}')
else:
    print('Package not found')
"
echo.
pause
goto menu
