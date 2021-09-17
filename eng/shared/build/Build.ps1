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
[string] $MsBuildInitScriptPath = ""

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

    if ( $UseMsbuild ) {
        & msbuild /t:Clean /p:Configuration=$configuration /m
    } else {
        & dotnet clean -p:Configuration=$configuration -v:m
    }

    if ($LASTEXITCODE -ne 0 ) { throw "Clean failed." }

    if (Test-Path "artifacts\bin\Debug" -PathType Container ) {
        Remove-Item "artifacts\bin\Debug\*.nupkg"
    }

    if ( Test-Path $PropsFilePath ) {
        Remove-Item $PropsFilePath
    }
}

function CreateVersionFile() {
    $timestamp = [System.DateTime]::Now.ToString('MMdd.HHmm')
        
    if ( $Local ) {
        # Local build with timestamp-based version and randomized package number.
        $packageVersion = "`$(MainVersion)-local-$([System.DateTime]::Now.Year)$timestamp-$([string]::Format( "{0:x8}", $(Get-Random) ) )-$configuration"
        $assemblyVersion = "`$(MainVersion)$timestamp"
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
    if ( $UseMsbuild ) {
        & msbuild /t:Restore /p:Configuration=$configuration /m
    } else {
        & dotnet restore -p:Configuration=$configuration
    }
    
    if ($LASTEXITCODE -ne 0 ) { throw "Restore failed." }
}

function Pack() {
    if ( $UseMsbuild ) {
        & msbuild /t:Pack /p:Configuration=$configuration /m
    } else {
        & dotnet pack -p:Configuration=$configuration --nologo --no-restore
    }

    if ($LASTEXITCODE -ne 0 ) { throw "Build failed." }

    Write-Host "Build successful" -ForegroundColor Green
}

function CopyToPublishDir() {
    & dotnet build eng\CopyToPublishDir.proj --nologo --no-restore
    if ($LASTEXITCODE -ne 0 ) { throw "Copying to publish directory failed." }

    Write-Host "Copying to publish directory successful" -ForegroundColor Green
}

function Sign() {
    & .\eng\shared\deploy\SignAndVerify.ps1 '$ProductName'

    if ($LASTEXITCODE -ne 0 ) { throw "Package signing and verification failed." }

    Write-Host "Package signing and verification successful" -ForegroundColor Green
}

function Test() {
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
            & msbuild /t:Test /p:CollectCoverage=True /p:CoverletOutput="$testResultsDir\" /p:Configuration=$configuration /m:1
        } else {
            & dotnet test -p:CollectCoverage=True -p:CoverletOutput="$testResultsDir\" -p:Configuration=$configuration -m:1 --nologo --no-restore
        }
        
        
        if ($LASTEXITCODE -ne 0 ) { throw "Tests failed." }
    
        # Detect gaps in code coverage.
        & ./tools/postsharp-eng coverage warn "$testResultsDir\coverage.json"
        if ($LASTEXITCODE -ne 0 ) { throw "Test coverage has gaps." }
    } else {
        # Executing tests without test coverage
        if ( $UseMsbuild ) {
            & msbuild /t:Test /p:Configuration=$configuration /m
        } else {
            & dotnet test  -p:Configuration=$configuration --nologo --no-restore
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
    Test
}
