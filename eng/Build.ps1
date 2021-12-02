# This is the backward-compatibility script for TeamCity and it should be deleted after the build configuration has been ported.

param (
    # Creates a numbered build (typically internal builds on a build server).
    [Parameter(Mandatory=$false, ValueFromPipeline=$false )]
    [int] $Numbered = -1,

    # Creates a release build instead of a debug one.
    [switch] $Release = $false,

    # Creates a local build with a version number based on a timestamp (typically a development build).
    [switch] $Local = $false,

    # Creates a public build.
    [switch] $Public = $false,

    # Sings the public packages (doesn't work without -Public -Release).
    [switch] $Sign = $false,

    # Creates $(ProductName)Version.props but does not build the project.
    [switch] $Prepare = $false,

    # Runs the test suite.
    [switch] $Test = $false
)

if ( $env:VisualStudioVersion -eq $null ) {
    Import-Module "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
    Enter-VsDevShell -VsInstallPath "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\" -StartInPath $(Get-Location)
}

# Map the old arguments to the new one
$arguments = @()
if ( $Test ) {
    $arguments += "test"
} else {
    $arguments += "build"
}

if ( $Public ) {
    $arguments += "--public" 
} elseif ( $Numbered -gt 0 ) {
    $arguments += "--numbered"
    $arguments += $Numbered
}

if ( $Release ) {
    $arguments += "--configuration"
    $arguments += "Release"
}

if ( $Sign ) {
    $arguments += "--sign"
}

$arguments += "--zip"

Write-Host "Update your command line to the new format. The new arguments are: $arguments"
& dotnet run --project .\eng\src\BuildCaravela.csproj -- $arguments

exit $LASTEXITCODE
