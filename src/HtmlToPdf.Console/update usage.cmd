@echo off

REM set errorlevel to 0
ver > nul

set tempOutputFile=%~dp0..\..\USAGE_temp.md

if exist "%tempOutputFile%" del "%tempOutputFile%"

set outputFile="%~dp0..\..\USAGE.md"
"%~dp0bin\Debug\net472\HtmlToPdf.Console.exe" --help >"%tempOutputFile%" 2>&1
if %errorlevel% neq 1 echo Failed to write usage to USAGE.md && exit /b %errorlevel%

setlocal EnableDelayedExpansion

for /f "delims=" %%n in ('find /c /v "" %tempOutputFile%') do set "len=%%n"
set "len=!len:*: =!"

<%tempOutputFile% (
  for /l %%l in (1 1 !len!) do (
    set "line="
    set /p "line="
    if %%l gtr 2 echo(!line!  
  )
) > %outputFile%

del "%tempOutputFile%"