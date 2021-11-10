param (

    [string] $otherArgs,

# LEGACY. Creates a numbered build (typically internal builds on a build server).
    [Parameter(Mandatory=$false, ValueFromPipeline=$false )]
    [int] $Numbered = -1,

# LEGACY. Creates a release build instead of a debug one.
    [switch] $Release = $false,

# LEGACY. Creates a local build with a version number based on a timestamp (typically a development build).
    [switch] $Local = $false,

# LEGACY. Creates a public build.
    [switch] $Public = $false,

# LEGACY. Sings the public packages (doesn't work without -Public -Release).
    [switch] $Sign = $false,

# LEGACY. Creates $(ProductName)Version.props but does not build the project.
    [switch] $Prepare = $false,

# LEGACY. Runs the test suite.
    [switch] $Test = $false
)

if ( $env:VisualStudioVersion -eq $null ) {
    Import-Module "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
    Enter-VsDevShell 50da0b83 -StartInPath $(Get-Location)
}

if ( $Release -or $Local -or $Numbered -gt 0 -or $Public ) {
    
    
    
    # Map the old arguments to the new one
    if ( $Test ) {
        $command = "test"
    } else {
        $command = "build"
    }
    
    if ( $Public ) {
        $version = "--public-build" 
    } elseif ( $Numbered > 0 ) {
        $version = "--versioned-build $Numbered"
    }

    if ( $Release ) {
        $configuration = "--configuration Release"
    } else {
        $configuration = ""
    }
    
    Write-Host "Update your command line to the new format. The new arguments are: '$command $version $configuration'"
    & dotnet run --project .\eng\src\Build.csproj -- $command $version $configuration
} else {
    # Use the new command line
    & dotnet run --project .\eng\src\Build.csproj -- $otherArgs    
}


