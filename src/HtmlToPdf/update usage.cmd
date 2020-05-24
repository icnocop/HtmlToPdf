@echo off

REM set errorlevel to 0
ver > nul

"%~dp0HtmlToPdf.exe" --help >"%~dp0..\..\..\..\USAGE.md" 2>&1
if %errorlevel% neq 1 echo Failed to write usage to USAGE.md && exit /b %errorlevel%