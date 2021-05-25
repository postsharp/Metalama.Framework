$ErrorActionPreference = "Stop"

If ( -Not ( Test-Path -Path ".\.git" ) ) {
    throw "This script has to run in a GIT repository root!"
}

$EngineeringDirectory = ".engineering"

git subtree add --prefix $EngineeringDirectory https://postsharp@dev.azure.com/postsharp/Caravela/_git/Caravela.Engineering master --squash


Get-ChildItem ".\$EngineeringDirectory\linked\root\" | 
Foreach-Object {
    New-Item -Path ".\$($_.Name)" -ItemType SymbolicLink -Value $_.FullName
}