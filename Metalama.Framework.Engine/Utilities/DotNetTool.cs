// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Extensibility;
using Metalama.Framework.Engine.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Metalama.Framework.Engine.Utilities;

[PublicAPI]
public sealed class DotNetTool
{
    private readonly IPlatformInfo _platformInfo;
    private readonly string _path;

    public DotNetTool( GlobalServiceProvider serviceProvider )
    {
        this._platformInfo = serviceProvider.GetRequiredBackstageService<IPlatformInfo>();

        this._path = Environment.GetEnvironmentVariable( "PATH" ) ?? string.Empty;
        const string riderPath = "/lib/ReSharperHost/";
        
        if ( RuntimeInformation.IsOSPlatform( OSPlatform.Linux ) && this._path.ContainsOrdinal( riderPath ) )
        {
            var logger = serviceProvider.GetLoggerFactory().GetLogger( "DotNetTool" );

            // When Rider is installed on Linux, it adds its own dotnet with no SDKs to its PATH.
            // This breaks our dotnet commands, so we remove any paths related to Rider from the PATH for our tools.
            // The path can look like the following, depending on whether Rider was installed using snap or the JetBrains Toolbox:
            // /snap/rider/408/lib/ReSharperHost/linux-x64/dotnet/dotnet
            // /home/petro/.local/share/JetBrains/Toolbox/apps/rider/lib/ReSharperHost/linux-x64/dotnet/dotnet

            var paths = this._path.Split( ':' ).Where( path => !path.ContainsOrdinal( riderPath ) );
            var newPath = string.Join( ":", paths );

            logger.Info?.Log( $"Changing PATH from '{this._path}' to '{newPath}'." );

            this._path = newPath;
        }
    }

    public void Execute( string arguments, string? workingDirectory = null, int timeout = 30_000 )
    {
        var startInfo = new ProcessStartInfo( this._platformInfo.DotNetExePath, arguments )
        {
            // We cannot call dotnet.exe with a \\?\-prefixed path because MSBuild would fail.
            WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,

            // We must avoid passing the following environment variables to the child process, otherwise there can be a mismatch
            // between SDK versions and the build will fail.
            Environment = { { "DOTNET_ROOT_X64", null }, { "MSBUILD_EXE_PATH", null }, { "MSBuildSDKsPath", null }, { "PATH", this._path } }
        };

        var process = new Process() { StartInfo = startInfo };

        var lines = new List<string>();

        void OnProcessDataReceived( object sender, DataReceivedEventArgs e )
        {
            lines.Add( e.Data ?? "" );
        }

        process.OutputDataReceived += OnProcessDataReceived;
        process.ErrorDataReceived += OnProcessDataReceived;

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();

        if ( !process.WaitForExit( timeout ) )
        {
            // The process did not complete within the given time.

            try
            {
                process.Kill();
            }
            catch
            {
                // ignored
            }

            throw new AssertionFailedException( $"The process '{startInfo.FileName} {startInfo.Arguments}' did not complete in {timeout / 1000f} s." );
        }

        if ( process.ExitCode != 0 )
        {
            throw new InvalidOperationException(
                $"Error calling `\"{this._platformInfo.DotNetExePath}\" {arguments}` in `{startInfo.WorkingDirectory}` returned {process.ExitCode}. Process output:"
                + Environment.NewLine + Environment.NewLine + string.Join( Environment.NewLine, lines ) );
        }
    }
}