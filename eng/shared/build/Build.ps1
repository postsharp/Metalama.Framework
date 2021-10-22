﻿<#
.SYNOPSIS
    This is the main build script. It is not required to run it before loading the projects in the IDE.
.DESCRIPTION
    Main use cases:
        1. Create development NuGet packages:
            Build.ps1 -Local
        2. Run the complete test suite in a development environment:
            Build.ps1 -Local -Test
        3. TeamCity: build debug packages and run tests:
            Build.ps1 -Numbered <NUMBER> -Test
        4. TeamCity: build release packages and run tests:
            Build.ps1 -Public -Release -Sign -Test

#>

param ( 

# Defines the name of the product contained in the repository. Set by the facade Build.ps1 script.
[Parameter(Mandatory=$true)] [string] $ProductName,

# Creates a release build instead of a debug one.
[switch] $Release = $false,

# Creates a local build with a version number based on a timestamp (typically a development build).
[switch] $Local = $false, 

# Creates a numbered build (typically internal builds on a build server).
[int] $Numbered = -1, 

# Creates a public build.
[switch] $Public = $false,

# Sings the public packages (doesn't work without -Public -Release).
[switch] $Sign = $false,

# Creates $(ProductName)Version.props but does not build the project.
[switch] $Prepare = $false,

# Runs the test suite.
[switch] $Test = $false,

# Verifies the test coverage (requires -Test).
[switch] $Coverage = $false,

# Uses msbuild command instead of dotnet command. Set by the facade Build.ps1 script.
[switch] $UseMsBuild = $false,

# Script to initialize the MSBuild shell environment. Mandatory when -UseMsBuild is set. Set by the facade Build.ps1 script.
[string] $MsBuildInitScriptPath = "",

# Ordered list of solutions to be tested. 
[Parameter(Mandatory=$False)]
[string[]] $TestSolutions=@(),

# Build verbosity
[Parameter(Mandatory=$False)]
[string] $Verbosity="m"


)

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

# Check required properties.
If ( [string]::IsNullOrEmpty( $ProductName ) ) {
    throw "Product name is not set."
}

if ( $UseMsBuild -and [string]::IsNullOrEmpty( $MsBuildInitScriptPath ) ) {
    throw "MSBuild init script path not set when MSBuild should be used."
}

$PropsFilePath = "eng\$($ProductName)Version.props"

if ( $Release ) {
    $configuration = "release"
} else {
    $configuration = "debug"
}

function CheckPrerequisities() {
    & .\eng\shared\style\LinkConfiguration.ps1 -Check

    if ($LASTEXITCODE -ne 0 ) { throw "Symbolic links verification failed." }

    if ( $UseMsBuild ) {
        # https://stackoverflow.com/questions/2124753/how-can-i-use-powershell-with-the-visual-studio-command-prompt
        cmd /c "`"$MsBuildInitScriptPath`" & set" |
        foreach {
            if ($_ -match "=") {
                $v = $_.split("=", 2); set-item -force -path "ENV:\$($v[0])" -value "$($v[1])"
            }
        }

        if ($LASTEXITCODE -ne 0 ) { throw "MSBuild initialization failed." }
    }
}

function Clean() {

    dir obj -Recurse | rd -Recurse -Force
    dir bin -Recurse | rd -Recurse -Force
    
    If ( Test-Path -Path "artifacts" ) {
        rd "artifacts" -Recurse -Force
    }

    if ( Test-Path $PropsFilePath ) {
        Remove-Item $PropsFilePath
    }
}

function CreateVersionFile() {
    $timestamp = [System.DateTime]::Now.ToString('MMdd.HHmmss')
    
    if ( $Local ) {
        # Local build with timestamp-based version and randomized package number. For the assembly version we use a local incremental file stored in the user profile.
        $localVersionDirectory="$Env:APPDATA\Caravela.Engineering"
        $localVersionFile = "$localVersionDirectory\$ProductName.version"
        if ( Test-Path $localVersionFile ) {
            $localVersion =  ((Get-Content $localVersionFile) -as [int]) + 1
        } else {
            $localVersion = 1
        }
        if ( $localVersion -lt 1000 ) { $localVersion = 1000 }


        if ( -Not (Test-Path $localVersionDirectory) ) {
            md $localVersionDirectory | Out-Null
        }
        Set-Content $localVersionFile $localVersion
                
        $packageVersionSuffix = "local-$localVersion-$([System.DateTime]::Now.Year)$timestamp-$([string]::Format( "{0:x8}", $(Get-Random) ) )-$configuration"
        $packageVersion = "`$(MainVersion)-$packageVersionSuffix"
        $assemblyVersion = "`$(MainVersion).$localVersion"

        Write-Host "PackageVersion = *-$packageVersionSuffix, AssemblyVersion=*.$localVersion"
       

    } elseif ( $Numbered -ge 0 ) {
        # Build server build with a build number given by the build server
        $packageVersion = "`$(MainVersion)-build-$configuration.$Numbered"
        $assemblyVersion = "`$(MainVersion).$Numbered"
    } elseif ( $Public ) {
        # Public build
        $packageVersion = "`$(MainVersion)`$(PackageVersionSuffix)"
        $assemblyVersion = "`$(MainVersion)"
    } else {
        Throw "One of the following flags must be used: -local, -numbered or -public"
    }

    $artifactsDir = "`$(MSBuildThisFileDirectory)..\artifacts\bin\$configuration"

    $props = @"
<!-- This file is generated by eng\shared\build\Build.ps1 -->
<Project>
    <Import Project="MainVersion.props" />
    <PropertyGroup>
        <$($ProductName)Version>$packageVersion</$($ProductName)Version>
        <$($ProductName)AssemblyVersion>$assemblyVersion</$($ProductName)AssemblyVersion>
    </PropertyGroup>
    <PropertyGroup>
        <!-- Adds the local output directories as nuget sources for referencing projects. -->
        <RestoreAdditionalProjectSources>`$(RestoreAdditionalProjectSources);$artifactsDir</RestoreAdditionalProjectSources>
    </PropertyGroup>
</Project>
"@

    New-Item $PropsFilePath -Value $props | Out-Null
}

function Restore() {

    Write-Host "------ Restoring ---------------------------------" -ForegroundColor Cyan
    
    if ( -not(Test-Path "artifacts\bin\$configuration" -PathType Container) ) {
        mkdir "artifacts\bin\$configuration" | Out-Null
    }
    
    if ( $UseMsbuild ) {
        & msbuild /t:Restore /p:Configuration=$configuration /m /v:$Verbosity
    } else {
        & dotnet restore -p:Configuration=$configuration -v:$Verbosity
    }
    
    if ($LASTEXITCODE -ne 0 ) { throw "Restore failed." }
    
    Write-Host "Restore successful" -ForegroundColor Green
}

function Pack() {

    Write-Host "------ Building ---------------------------------" -ForegroundColor Cyan

    if ( $UseMsbuild ) {
        & msbuild /t:Pack /p:Configuration=$configuration /m /v:$Verbosity
    } else {
        & dotnet pack -p:Configuration=$configuration -v:$Verbosity --nologo --no-restore
    }

    if ($LASTEXITCODE -ne 0 ) { throw "Build failed." }

    Write-Host "Build successful" -ForegroundColor Green
}

function CopyToPublishDir() {
    
    Write-Host "------ Publishing ---------------------------------" -ForegroundColor Cyan
    
    # TODO: Redesign this to remove the back reference. 
    
    & dotnet build eng\CopyToPublishDir.proj --nologo --no-restore -v:$Verbosity
    if ($LASTEXITCODE -ne 0 ) { throw "Copying to publish directory failed." }

    Write-Host "Copying to publish directory successful" -ForegroundColor Green
}

function Sign() {

    Write-Host "------ Signing ---------------------------------" -ForegroundColor Cyan
    
    & .\eng\shared\deploy\SignAndVerify.ps1 '$ProductName'

    if ($LASTEXITCODE -ne 0 ) { throw "Package signing and verification failed." }

    Write-Host "Package signing and verification successful" -ForegroundColor Green
}

function Test() {
     param ( [string] $solution )
     
      Write-Host "------ Testing $solution ---------------------------------" -ForegroundColor Cyan
     
     $testResultsDir = $(Get-Location).Path + "\TestResults"

    if ( $Coverage ) {
         # Removing the TestResults directory so that we reset the code coverage information.
        if ( Test-Path $testResultsDir ) {
            Remove-Item $testResultsDir -Recurse -Force
        }
    
        # Building dotnet tools.
        & ./eng/shared/tools/Build.ps1
    
        # Executing tests with code coverage enabled.
        if ( $UseMsbuild ) {
            & msbuild $solution /t:Test /p:CollectCoverage=True /p:CoverletOutput="$testResultsDir\" /p:Configuration=$configuration /m:1 /v:$Verbosity
        } else {
            & dotnet test -p:CollectCoverage=True -p:CoverletOutput="$testResultsDir\" -p:Configuration=$configuration -m:1 --nologo -v:$Verbosity $solution   
        }
        
        
        if ($LASTEXITCODE -ne 0 ) { throw "Tests failed." }
    
        # Detect gaps in code coverage.
        & ./tools/postsharp-eng coverage warn "$testResultsDir\coverage.json"
        if ($LASTEXITCODE -ne 0 ) { throw "Test coverage has gaps." }
    } else {
        # Executing tests without test coverage
        if ( $UseMsbuild ) {
            & msbuild /t:Test /p:Configuration=$configuration /m /v:$Verbosity $solution 
        } else {
            & dotnet test  -p:Configuration=$configuration --nologo -v:$Verbosity $solution 
        }
        if ($LASTEXITCODE -ne 0 ) { throw "Tests failed." }
    }


    Write-Host "Tests successful" -ForegroundColor Green
}

CheckPrerequisities
Clean
CreateVersionFile
Restore

if ( -not( $Prepare ) ) {
    Pack

    if ( $Public -and $Release ) {
        CopyToPublishDir

        if ( $Sign ) {
            Sign
        }
    }
}

if ( $Test ) {
    if ( $TestSolutions.Count -eq 0 ) {
        Test 
    } else {
        foreach ( $solution in $TestSolutions ) {
        Test $solution    
        }
    }
}
