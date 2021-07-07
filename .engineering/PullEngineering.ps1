# This script initializes or updates common engineering in a Caravela-related GIT repository.
# Usage: Copy this file to the repository root and execute.

# Stop after first error.
$ErrorActionPreference = "Stop"

# Check that we are in the root of a GIT repository.
If ( -Not ( Test-Path -Path ".\.git" ) ) {
    throw "This script has to run in a GIT repository root! Usage: Copy this file to the root of the repository and execute. The file deletes itself upon success."
}

# Update/initialize the engineering subtree.
$EngineeringDirectory = ".engineering"

If ( ( Test-Path -Path $EngineeringDirectory ) ) {
    $SubtreeCommand = "pull"
} else {
    $SubtreeCommand = "add"
}

& git subtree $SubtreeCommand --prefix $EngineeringDirectory https://postsharp@dev.azure.com/postsharp/Caravela/_git/Caravela.Engineering master --squash

If ( $LastExitCode -Ne 0 ) {
    throw "Failed to $SubtreeCommand subtree."
}

# Clean-up existing links or copies of linked files. This also solves issues with invalid merges or updates,
# where a link file is replaced by a file containing the original target path.
# Keep this clean-up part up to date, so that this script can be executed on any repository in any state.
Remove-Item ".\.editorconfig"

Get-ChildItem ".\" -Filter "*.sln.DotSettings" | 
Foreach-Object {
    Remove-Item $_.FullName
}

# Link files.
# If the creation of the symlinks fails, either the script needs to be executed with elevation, or the Windows Developer Mode needs to be enabled.
New-Item -Path ".\.editorconfig" -ItemType SymbolicLink -Value ".\$EngineeringDirectory\style\.editorconfig"

Get-ChildItem ".\" -Filter "*.sln" | 
Foreach-Object {
    New-Item -Path "$($_.FullName).DotSettings" -ItemType SymbolicLink -Value ".\$EngineeringDirectory\style\sln.DotSettings"
}

# Remove the executed copy of this script.
Remove-Item -LiteralPath $MyInvocation.MyCommand.Path -Force