@echo off
echo Starting %APP_NAME%...
cd /d "%~dp0backend"
python run.py
pause
