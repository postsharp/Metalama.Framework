@echo off

echo Publishing Caravela version %CARAVELA_VERSION%

cd artifacts\bin\Release

dotnet nuget push -s https://nuget.postsharp.net/nuget/caravela/ -k %NUGET_KEY% Caravela.Framework.Redist.%CARAVELA_VERSION%.nupkg 
dotnet nuget push -s https://nuget.postsharp.net/nuget/caravela/ -k %NUGET_KEY% Caravela.Framework.%CARAVELA_VERSION%.nupkg 
dotnet nuget push -s https://nuget.postsharp.net/nuget/caravela/ -k %NUGET_KEY% Caravela.Framework.Sdk.%CARAVELA_VERSION%.nupkg 
dotnet nuget push -s https://nuget.postsharp.net/nuget/caravela/ -k %NUGET_KEY% Caravela.Framework.Impl.%CARAVELA_VERSION%.nupkg 

cd ..\..\..