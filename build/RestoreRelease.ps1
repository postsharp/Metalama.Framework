# This script is used by TeamCity to restore packages for the release build. 
# It is designed to fail if there is any dependency on a package that has not been published
# to the public nuget.org feed. For a development restore, just do `dotnet restore`.

# Working directory has to be the repository root.

# Stop after first error.
$ErrorActionPreference = "Stop"

# Check that we are in the root of a GIT repository.
If ( -Not ( Test-Path -Path ".\.git" ) ) {
    throw "This script has to run in a GIT repository root!"
}

$NuGetPackages=.\.packages

if ( Test-Path $NuGetPackages ) {
    Remove-Item $NuGetPackages -Force -Recurse
}

& dotnet restore -p:Configuration=Release -p:Obfuscate=True -s:https://api.nuget.org/v3/index.json