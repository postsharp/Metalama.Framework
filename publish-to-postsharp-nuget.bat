@echo off

IF "%~1" == "" GOTO Error
IF "%~2" == "" GOTO Error

set "CARAVELA_VERSION=%~1"
set "NUGET_KEY=%~2"

dotnet pack -c Release -p:Obfuscate=True

echo Publishing Caravela version %CARAVELA_VERSION%

cd artifacts\bin\Release

dotnet nuget push -s https://nuget.postsharp.net/nuget/caravela/ -k %NUGET_KEY% Caravela.Framework.Redist.%CARAVELA_VERSION%.nupkg 
dotnet nuget push -s https://nuget.postsharp.net/nuget/caravela/ -k %NUGET_KEY% Caravela.Framework.%CARAVELA_VERSION%.nupkg 
dotnet nuget push -s https://nuget.postsharp.net/nuget/caravela/ -k %NUGET_KEY% Caravela.Framework.Sdk.%CARAVELA_VERSION%.nupkg 
dotnet nuget push -s https://nuget.postsharp.net/nuget/caravela/ -k %NUGET_KEY% Caravela.Framework.Impl.%CARAVELA_VERSION%.nupkg 

cd ..\..\..

goto :eof

:Error
echo Command line incorrect, usage: %0 version nuget_key, e.g. %0 1.0 ABCDEFGHIJKLMNOPQRST
echo Make sure to also update the version in Directory.Build.props and that you are connected to the PostSharp VPN