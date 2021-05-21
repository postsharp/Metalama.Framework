// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Options;
using System;
using System.Diagnostics;

namespace Caravela.Framework.Impl.Utilities
{
    /// <summary>
    /// Exposes the <see cref="AttachDebugger"/> method.
    /// </summary>
    internal static class DebuggingHelper
    {
        private static readonly object _sync = new();
        private static volatile bool _attachDebuggerRequested;

        public static ProcessKind ProcessKind
            => Process.GetCurrentProcess().ProcessName.ToLowerInvariant() switch
            {
                "devenv" => ProcessKind.DevEnv,
                "servicehub.roslyncodeanalysisservice" => ProcessKind.RoslynCodeAnalysisService,
                "csc" => ProcessKind.Compiler,
                "dotnet" =>
                    Environment.CommandLine.Contains( "JetBrains.ReSharper.Roslyn.Worker.exe" ) ? ProcessKind.Resharper :
                    Environment.CommandLine.Contains( "VBCSCompiler.dll" ) ? ProcessKind.Compiler :
                    ProcessKind.Other,
                _ => ProcessKind.Other
            };

        public static void RequireCaravelaCompiler()
        {
            if ( ProcessKind == ProcessKind.Compiler && !CaravelaCompilerInfo.IsActive )
            {
                throw new AssertionFailedException( "Caravela is running in the vanilla C# compiler instead of the customized one." );
            }
        }
  
        /// <summary>
        /// Attaches the debugger to the current process if requested.
        /// </summary>
        public static void AttachDebugger( IDebuggingOptions projectOptions )
        {
            var mustAttachDebugger =
                ProcessKind switch
                {
                    ProcessKind.DevEnv => projectOptions.DebugIdeProcess,
                    ProcessKind.RoslynCodeAnalysisService => projectOptions.DebugAnalyzerProcess,
                    ProcessKind.Resharper => projectOptions.DebugAnalyzerProcess,
                    ProcessKind.Compiler => projectOptions.DebugCompilerProcess,
                    _ => false
                };

            if ( mustAttachDebugger && !_attachDebuggerRequested )
            {
                lock ( _sync )
                {
                    if ( !_attachDebuggerRequested )
                    {
                        // We try to request to attach the debugger a single time, even if the user refuses or if the debugger gets
                        // detaches. It makes a better debugging experience.
                        _attachDebuggerRequested = true;

                        if ( !Debugger.IsAttached )
                        {
                            Debugger.Launch();
                        }
                    }
                }
            }
        }
    }
}