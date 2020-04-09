##-----------------------------------------------------------------------
## <copyright file="SetBuildNumber.ps1"></copyright>
##-----------------------------------------------------------------------

# Enable -Verbose option
[CmdletBinding()]

# Set a flag to force verbose as a default
$VerbosePreference ='Continue' # equiv to -verbose

$majorVersion = 0
$minorVersion = 0
$firstBuildYear = 2020

$currentYear = Get-Date -Format yyyy
$buildYear = [Convert]::ToInt32($currentYear)
$currentMonthDay = Get-Date -Format MMdd
$buildVersion = (($buildYear - $firstBuildYear) * 1200) + ([Convert]::ToInt32($currentMonthDay))

Write-Host "##vso[build.updatebuildnumber]$majorVersion.$minorVersion.$buildVersion.$env:BUILD_COUNTER"
Write-Host "##vso[task.setvariable variable=SemVer]$majorVersion.$minorVersion.$buildVersion-revision$env:BUILD_COUNTER"