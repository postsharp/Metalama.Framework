<#
.SYNOPSIS
    This script build the engineering tools.
#>

$ErrorActionPreference = "Stop"

trap
{
    Write-Error $PSItem.ToString()
    exit 1
}

& dotnet build $PSScriptRoot/src/PostSharp.Engineering.BuildTools.sln
