<#
.SYNOPSIS
    Builds and runs the Melissa SSN Name Match Cloud API .NET sample.

.DESCRIPTION
    This script builds SSNNameMatchDotnet with dotnet publish, then runs the resulting
    executable, passing along the license and (if supplied) the SSN.

    Overall flow:
      1. Resolve the license (parameter, prompt, or MD_LICENSE environment variable).
      2. Publish SSNNameMatchDotnet in Release configuration to
         .\SSNNameMatchDotnet\Build.
      3. Run the built executable: one-shot mode if an SSN was supplied, otherwise
         interactive mode (the .NET program prompts for the SSN).

.PARAMETER ssn
    Social Security Number to look up in one-shot mode.

.PARAMETER license
    License string. Resolved in this order:
      1. This parameter.
      2. An interactive prompt, if the parameter was not supplied.
      3. The MD_LICENSE environment variable, if the prompt was left blank.
    Note that the environment variable is the last resort, not the first: running
    without -license always prompts, even when MD_LICENSE is set.

.PARAMETER quiet
    Accepted for parity with other sample scripts; not currently used to suppress output.

.EXAMPLE
    .\SSNNameMatchDotnet.ps1 -license "your-license"

.EXAMPLE
    .\SSNNameMatchDotnet.ps1 -ssn "111223333" -license "your-license"
#>

######################### Parameters ##########################
param(
    $ssn = '',
    $license = '',
    [switch]$quiet = $false
    )

# Uses the location of the .ps1 file
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
# No SSN supplied -> run interactively; otherwise pass it through for one-shot mode.
if ([string]::IsNullOrEmpty($ssn)) {
  dotnet $BuildPath\SSNNameMatchDotnet.dll --license $license 
}
else {
  dotnet $BuildPath\SSNNameMatchDotnet.dll --license $license --ssn $ssn
}
