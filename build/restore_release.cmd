@echo off

rem This script is used by TeamCity to restore packages for the release build. 
rem It is designed to fail if there is any dependency on a package that has not been published
rem to the public nuget.org feed. For a development restore, just do `dotnet restore`.

rem Working directory has to be the repository root.

set NUGET_PACKAGES=%cd%\.packages

if exist %NUGET_PACKAGES%\ rd %NUGET_PACKAGES% /q /s
dotnet restore -p:Configuration=Release -p:Obfuscate=True -s:https://api.nuget.org/v3/index.json