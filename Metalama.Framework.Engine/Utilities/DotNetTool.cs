﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Extensibility;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Metalama.Framework.Engine.Utilities;

public class DotNetTool
{
    private readonly IPlatformInfo _platformInfo;

    public DotNetTool( IServiceProvider serviceProvider )
    {
        this._platformInfo = serviceProvider.GetRequiredBackstageService<IPlatformInfo>();
    }

    public void Execute( string arguments, string? workingDirectory = null )
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
            Environment = { { "DOTNET_ROOT_X64", null }, { "MSBUILD_EXE_PATH", null }, { "MSBuildSDKsPath", null } }
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
        process.WaitForExit();

        if ( process.ExitCode != 0 )
        {
            throw new InvalidOperationException(
                $"Error calling `\"{this._platformInfo.DotNetExePath}\" {arguments}` in `{startInfo.WorkingDirectory}` returned {process.ExitCode}. Process output:"
                + Environment.NewLine + Environment.NewLine + string.Join( Environment.NewLine, lines ) );
        }
    }
}