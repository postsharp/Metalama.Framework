$process = Start-Process -WorkingDirectory  "$PSScriptRoot/eng/src" -NoNewWindow -PassThru -FilePath dotnet -ArgumentList "run $args" 
$process.WaitForExit()
exit $process.ExitCode

