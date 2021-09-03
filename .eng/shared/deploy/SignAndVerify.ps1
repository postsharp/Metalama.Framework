
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

& ./.eng/shared/tools/BuildTools.ps1
& ./.eng/shared/tools/RestoreTools.ps1 SignClient 

$CurrentDir = $(get-location).Path

& ./tools/SignClient Sign --baseDirectory $CurrentDir\artifacts\publish\ --input *.nupkg --config $CurrentDir\.eng\shared\deploy\signclient-appsettings.json --name $ProjectName --user sign-caravela@postsharp.net --secret $Env:SIGNSERVER_SECRET

if (!$?) {
	throw "Signing failed."
}

& ./tools/postsharp-eng nuget verify -d artifacts\publish

if (!$?) {
	throw "Verification failed."
}