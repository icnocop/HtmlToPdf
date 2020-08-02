@echo off

set xsd="C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.6.1 Tools\xsd.exe"

set var=%xsd% /nologo Outline.xml
echo %var%
%var%

set var=%xsd% /nologo Outline.xsd /classes /namespace:HtmlToPdf.Console.Outline.Xml
echo %var%
%var%
if errorlevel neq 0 goto Failed

set var=powershell -Command "(gc Outline.cs) -replace 'item\[\]', 'List<item>' | Out-File -encoding UTF8 Outline.cs"
echo %var%
%var%

set var=powershell -Command "(gc Outline.cs) -replace 'item1', 'children' | Out-File -encoding UTF8 Outline.cs"
echo %var%
%var%

set var=powershell -Command "(gc Outline.cs) -replace 'using System.Xml.Serialization;', 'using System.Collections.Generic;' | Out-File -encoding UTF8 Outline.cs"
echo %var%
%var%

echo
echo Success!
exit 0
:Failed
echo Failed!
exit 1