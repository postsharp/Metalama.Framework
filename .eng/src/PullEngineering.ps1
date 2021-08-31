# Stop after first error.
$ErrorActionPreference = "Stop"

# Check that we are in the root of a GIT repository.
If ( -Not ( Test-Path -Path ".\.git" ) ) {
    throw "This script has to run in a GIT repository root!"
}

# Update/initialize the engineering subtree.
$EngineeringDirectory = ".eng/src"

& git subtree pull --prefix $EngineeringDirectory https://postsharp@dev.azure.com/postsharp/Caravela/_git/Caravela.Engineering master --squash
