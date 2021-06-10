# This scripts creates a new set of nuget packages for local use.
# It generates a new version number, which is then read by Version.props so that it is available
# by other solutions that reference the NuGet packages.

$random1 = Get-Random 
$random2 = Get-Random

$random = "$random1$random2"

$props = "<Project><PropertyGroup><LocalBuildId>$random</LocalBuildId></PropertyGroup></Project>"

$LocalBuildIdProps = "..\LocalBuildId.props";

if ( Test-Path $LocalBuildIdProps ) {
    Remove-Item $LocalBuildIdProps
}

New-Item $LocalBuildIdProps -Value $props | Out-Null

& dotnet pack
& kill.ps1