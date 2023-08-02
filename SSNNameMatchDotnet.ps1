# Name:    SSNNameMatchCloudAPI
# Purpose: Execute the SSNNameMatchCloudAPI program

######################### Parameters ##########################
param(
    $ssn = '', 
    $license = '', 
    [switch]$quiet = $false
    )

# Uses the location of the .ps1 file 
# Modify this if you want to use 
$CurrentPath = $PSScriptRoot
Set-Location $CurrentPath
$ProjectPath = "$CurrentPath\SSNNameMatchDotnet"
$BuildPath = "$ProjectPath\Build"

If (!(Test-Path $BuildPath)) {
  New-Item -Path $ProjectPath -Name 'Build' -ItemType "directory"
}

########################## Main ############################
Write-Host "`n======================= Melissa SSN Name Match Cloud API =======================`n"

# Get license (either from parameters or user input)
if ([string]::IsNullOrEmpty($license) ) {
  $license = Read-Host "Please enter your license string"
}

# Check for License from Environment Variables 
if ([string]::IsNullOrEmpty($license) ) {
  $license = $env:MD_LICENSE 
}

if ([string]::IsNullOrEmpty($license)) {
  Write-Host "`nLicense String is invalid!"
  Exit
}

# Start program
# Build project
Write-Host "`n================================= BUILD PROJECT ================================"

dotnet publish -f="net7.0" -c Release -o $BuildPath SSNNameMatchDotnet\SSNNameMatchDotnet.csproj

# Run project
if ([string]::IsNullOrEmpty($ssn)) {
  dotnet $BuildPath\SSNNameMatchDotnet.dll --license $license 
}
else {
  dotnet $BuildPath\SSNNameMatchDotnet.dll --license $license --ssn $ssn
}
