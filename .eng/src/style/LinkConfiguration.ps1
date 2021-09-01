# This script will link the `.editorconfig` file to the repository root and the `sln.DotSettings` next to each `.sln` solution file in the repository with a name corresponding to the solution name.

# Stop after first error.
$ErrorActionPreference = "Stop"

trap
{
    Write-Error $PSItem.ToString()
    exit 1
}

# Check that we are in the root of a GIT repository.
If ( -Not ( Test-Path -Path ".\.git" ) ) {
    throw "This script has to run in a GIT repository root! Usage: Copy this file to the root of the repository and execute. The file deletes itself upon success."
}

# Update/initialize the engineering subtree.
$EngineeringDirectory = ".eng\src"

$EditorConfigFile = ".\.editorconfig"

if ( Test-Path -Path $EditorConfigFile ) {
    Remove-Item $EditorConfigFile
}

Get-ChildItem ".\" -Filter "*.sln.DotSettings" | 
Foreach-Object {
    Remove-Item $_.FullName
}

# Link files.
# If the creation of the symlinks fails, either the script needs to be executed with elevation, or the Windows Developer Mode needs to be enabled.
# We have to use the mklink command instead of the New-Command cmd-let, because the New-Command cmd-let requires elevation even with the Windows Developer Mode enabled.
# We have to execute the mklink command in a Windows Command Prompt process, because mklink is not a stand-alone executable, but a built-in command.
& cmd /c mklink $EditorConfigFile ".\$EngineeringDirectory\style\.editorconfig"

Get-ChildItem ".\" -Filter "*.sln" | 
Foreach-Object {
    & cmd /c mklink "$($_.FullName).DotSettings" ".\$EngineeringDirectory\style\sln.DotSettings"
}
