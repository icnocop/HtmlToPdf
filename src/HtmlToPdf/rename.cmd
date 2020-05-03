@echo off
if exist %~dp0wkhtmltopdf.exe del %~dp0wkhtmltopdf.exe
if %errorlevel% neq 0 exit /b %errorlevel%
ren %~dp0HtmlToPdf.exe wkhtmltopdf.exe
if %errorlevel% neq 0 exit /b %errorlevel%
if exist %~dp0wkhtmltopdf.exe.config del %~dp0wkhtmltopdf.exe.config
if %errorlevel% neq 0 exit /b %errorlevel%
ren %~dp0HtmlToPdf.exe.config wkhtmltopdf.exe.config
if %errorlevel% neq 0 exit /b %errorlevel%
if exist %~dp0wkhtmltopdf.pdb del %~dp0wkhtmltopdf.pdb
if %errorlevel% neq 0 exit /b %errorlevel%
ren %~dp0HtmlToPdf.pdb wkhtmltopdf.pdb
if %errorlevel% neq 0 exit /b %errorlevel%
if exist %~dp0wkhtmltopdf.xml del %~dp0wkhtmltopdf.xml
if %errorlevel% neq 0 exit /b %errorlevel%
ren %~dp0HtmlToPdf.xml wkhtmltopdf.xml