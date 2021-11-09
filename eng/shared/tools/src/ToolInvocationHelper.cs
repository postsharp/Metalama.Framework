using PostSharp.Engineering.BuildTools.Console;
using Spectre.Console;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace PostSharp.MSBuild
{
    internal static class ToolInvocationHelper
    {
        public static bool InvokeTool( ConsoleHelper console, string fileName, string commandLine,
            string workingDirectory,
            CancellationToken cancellationToken = default,
            params (string key, string value)[] environmentVariables )
        {
            int exitCode;
            if ( !InvokeTool( console, fileName, commandLine, workingDirectory, cancellationToken, out exitCode,
                environmentVariables ) )
            {
                return false;
            }
            else if ( exitCode != 0 )
            {
                console.WriteError( "The process \"{0}\" failed with exit code {1}.", fileName, exitCode );
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool InvokeTool( ConsoleHelper console, string fileName, string commandLine,
            string workingDirectory,
            CancellationToken cancellationToken,
            out int exitCode, params (string key, string value)[] environmentVariables )
        {
            return
                InvokeTool( console, fileName, commandLine, workingDirectory, cancellationToken, out exitCode,
                    s =>
                    {
                        if ( !string.IsNullOrWhiteSpace( s ) )
                        {
                            console.WriteError( s );
                        }
                    },
                    s =>
                    {
                        if ( !string.IsNullOrWhiteSpace( s ) )
                        {
                            console.WriteMessage( s );
                        }
                    },
                    environmentVariables );
        }

        // #16205 We don't allow cancellation here because there's no other working way to wait for a process exit
        // than Process.WaitForExit() on .NET Core when capturing process output.
        public static bool InvokeTool( ConsoleHelper console, string fileName, string commandLine,
            string workingDirectory,
            out int exitCode, out string output,
            params (string key, string value)[] environmentVariables )
        {
            StringBuilder stringBuilder = new();

            var success =
                InvokeTool( console, fileName, commandLine, workingDirectory, null, out exitCode,
                    s =>
                    {
                        lock ( stringBuilder )
                        {
                            stringBuilder.Append( s );
                            stringBuilder.Append( '\n' );
                        }
                    },
                    s =>
                    {
                        lock ( stringBuilder )
                        {
                            stringBuilder.Append( s );
                            stringBuilder.Append( '\n' );
                        }
                    },
                    environmentVariables );

            output = stringBuilder.ToString();

            return success && exitCode == 0;
        }

        private static bool InvokeTool( ConsoleHelper console, string fileName, string commandLine,
            string workingDirectory,
            CancellationToken? cancellationToken, out int exitCode, Action<string> handleErrorData,
            Action<string> handleOutputData, params (string key, string value)[] environmentVariables )
        {
            exitCode = 0;

#pragma warning disable CA1307 // There is no string.Contains that takes a StringComparison
            if ( fileName.Contains( new string( Path.DirectorySeparatorChar, 1 ) ) && !File.Exists( fileName ) )
            {
                console.WriteError( "Cannot execute \"{0}\": file not found.", fileName );
                return false;
            }
#pragma warning restore CA1307

            const int restartLimit = 3;
            var restartCount = 0;
            start:


            ProcessStartInfo startInfo =
                new()
                {
                    FileName = fileName,
                    Arguments = commandLine,
                    WorkingDirectory = workingDirectory,
                    ErrorDialog = false,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                };

            foreach ( var pair in environmentVariables )
            {
                startInfo.Environment[pair.key] = pair.value;
            }

            string toolName = Path.GetFileName( fileName );
            Process process = new() { StartInfo = startInfo };

            using ( ManualResetEvent stdErrorClosed = new(false) )
            using ( ManualResetEvent stdOutClosed = new(false) )
            {
                process.ErrorDataReceived += ( sender, args ) =>
                {
                    try
                    {
                        if ( args.Data == null )
                        {
                            stdErrorClosed.Set();
                        }
                        else
                        {
                            handleErrorData( args.Data );
                        }
                    }
                    catch ( Exception e )
                    {
                        console.Error.WriteException( e );
                    }
                };

                process.OutputDataReceived += ( sender, args ) =>
                {
                    try
                    {
                        if ( args.Data == null )
                        {
                            stdOutClosed.Set();
                        }
                        else
                        {
                            handleOutputData( args.Data );
                        }
                    }
                    catch ( Exception e )
                    {
                        console.Error.WriteException( e );
                    }
                };

                console.WriteMessage( "Executing \"{0}\" {1}", process.StartInfo.FileName,
                    process.StartInfo.Arguments );

                using ( process )
                {
                    try
                    {
                        process.Start();
                    }
                    catch ( Win32Exception e ) when ( (uint) e.NativeErrorCode == 0x80004005 )
                    {
                        if ( restartCount < restartLimit )
                        {
                            console.WriteWarning(
                                "Access denied when starting a process. This might be caused by an anti virus software. Waiting 1000 ms and restarting." );
                            Thread.Sleep( 1000 );
                            restartCount++;
                            goto start;
                        }

                        throw;
                    }

                    if ( !cancellationToken.HasValue )
                    {
                        process.BeginErrorReadLine();
                        process.BeginOutputReadLine();
                        process.WaitForExit();
                    }
                    else
                    {
                        using ( ManualResetEvent cancelledEvent = new(false) )
                        using ( ManualResetEvent exitedEvent = new(false) )
                        {
                            process.EnableRaisingEvents = true;
                            process.Exited += ( sender, args ) => exitedEvent.Set();

                            using ( cancellationToken.Value.Register( () => cancelledEvent.Set() ) )
                            {
                                process.BeginErrorReadLine();
                                process.BeginOutputReadLine();

                                if ( !process.HasExited )
                                {
                                    var signal = WaitHandle.WaitAny( new WaitHandle[] { exitedEvent, cancelledEvent } );

                                    if ( signal == 1 )
                                    {
                                        cancellationToken.Value.ThrowIfCancellationRequested();
                                    }
                                }
                            }
                        }
                    }

                    // We will wait for a while for all output to be processed.
                    if ( !cancellationToken.HasValue )
                    {
                        WaitHandle.WaitAll( new WaitHandle[] { stdErrorClosed, stdOutClosed }, 10000 );
                    }
                    else
                    {
                        var i = 0;
                        while ( !WaitHandle.WaitAll( new WaitHandle[] { stdErrorClosed, stdOutClosed }, 100 ) &&
                                i++ < 100 )
                        {
                            cancellationToken.Value.ThrowIfCancellationRequested();
                        }
                    }

                    exitCode = process.ExitCode;
                    return true;
                }
            }
        }
    }
}