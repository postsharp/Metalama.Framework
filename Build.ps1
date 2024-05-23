Start-Process -WorkingDirectory  "$PSScriptRoot/eng/src" -NoNewWindow -Wait -FilePath dotnet -ArgumentList "run $args" 
exit $LASTEXITCODE

