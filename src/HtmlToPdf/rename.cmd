@echo off

REM set errorlevel to 0
ver > nul

REM wkhtmltopdf.exe
copy /y "%~dp0HtmlToPdf.exe" "%~dp0wkhtmltopdf.exe"
if %errorlevel% neq 0 echo Failed to copy HtmlToPdf.exe to wkhtmltopdf.exe && exit /b %errorlevel%

REM wkhtmltopdf.exe.config
copy /y "%~dp0HtmlToPdf.exe.config" "%~dp0wkhtmltopdf.exe.config
if %errorlevel% neq 0 echo Failed to copy HtmlToPdf.exe.config to wkhtmltopdf.exe.config && exit /b %errorlevel%

REM wkhtmltopdf.pdb
copy /y "%~dp0HtmlToPdf.pdb" "%~dp0wkhtmltopdf.pdb
if %errorlevel% neq 0 echo Failed to copy HtmlToPdf.pdb to wkhtmltopdf.pdb && exit /b %errorlevel%

REM wkhtmltopdf.xml
copy /y "%~dp0HtmlToPdf.xml" "%~dp0wkhtmltopdf.xml
if %errorlevel% neq 0 echo Failed to copy HtmlToPdf.pdb to wkhtmltopdf.pdb && exit /b %errorlevel%