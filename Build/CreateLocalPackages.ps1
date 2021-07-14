﻿param ( [switch] $release = $false )

# This scripts creates a new set of nuget packages for local use.
# It generates a new version number, which is then read by Version.props so that it is available
# by other solutions that reference the NuGet packages.

$random1 = Get-Random 
$random2 = Get-Random

$random = "$random1$random2"

$props = "<Project><PropertyGroup><LocalBuildId>$random</LocalBuildId></PropertyGroup></Project>"

if (Test-Path "artifacts\bin\Debug" -PathType Container ) {
    del "artifacts\bin\Debug\*.nupkg"
}

if ( Test-Path LocalBuildId.props ) {
    Remove-Item LocalBuildId.props
}

New-Item LocalBuildId.props -Value $props | Out-Null

if ( $release ) {
$configuration = "Release"
$obfuscation = "True"
} else {
$configuration = "Debug"
$obfuscation = "False" 
}


& dotnet pack -p:Configuration=$configuration -p:Obfuscate=$obfuscation -p:UseLocalBuildId=True 
