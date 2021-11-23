
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


$ScriptDir = Split-Path $script:MyInvocation.MyCommand.Path

echo "Copying $ScriptDir to eng\shared"

Copy-Item $ScriptDir\* "eng\shared" -Force -Recurse -Exclude ".git" 