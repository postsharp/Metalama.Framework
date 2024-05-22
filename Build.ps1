& $Env:ProgramFiles\Docker\Docker\DockerCli.exe -SwitchWindowsEngine
(& dotnet nuget locals http-cache -c) | Out-Null
& dotnet run --project "$PSScriptRoot/eng/src/BuildMetalama.csproj" -- $args
exit $LASTEXITCODE

