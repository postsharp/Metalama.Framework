
Param ( [Parameter(Mandatory=$True)] [string] $ProjectName, [string] $InternalNuGetUrl = "https://nuget.postsharp.net/nuget/caravela/" )

# Stop after first error.
$ErrorActionPreference = "Stop"

trap
{
    Write-Error $PSItem.ToString()
    exit 1
}

# Check that we are in the root of a GIT repository.
If ( -Not ( Test-Path -Path ".\.git" ) ) {
    throw "This script has to run in a GIT repository root!"
}

& ./eng/shared/tools/Build.ps1
& ./eng/shared/tools/Restore.ps1 SignClient 

$CurrentDirectory = $(get-location).Path
$BaseDirectory = "$CurrentDirectory\artifacts\publish\"

function Sign() {
    param (
        [string] $InputFilter
    )

    if ( Test-Path "$BaseDirectory\$InputFilter" ) {
        & ./tools/SignClient Sign --baseDirectory $BaseDirectory --input $InputFilter --config $CurrentDirectory\eng\shared\deploy\signclient-appsettings.json --name $ProjectName --user sign-caravela@postsharp.net --secret $Env:SIGNSERVER_SECRET

        if (!$?) {
            throw "Signing failed."
        }
    }
}

function Verify() {
    param (
        [string] $InputFilter
    )

    if ( Test-Path "$BaseDirectory\$InputFilter" ) {
        & ./tools/postsharp-eng nuget verify -d artifacts\publish

        if (!$?) {
	        throw "Verification failed."
        }
    }
}

Sign "*.nupkg"
Verify "*.nupkg"

Sign "*.vsix"