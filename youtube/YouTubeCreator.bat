@echo off
title YouTube Auto Creator
color 0B
echo.
echo  ============================================
echo    YouTube Auto Creator - Full Application
echo  ============================================
echo.
echo  Starting application...
echo.
python "%~dp0app.py"
if errorlevel 1 (
    echo.
    echo  Error occurred. Make sure Python is installed.
    echo.
    pause
)
