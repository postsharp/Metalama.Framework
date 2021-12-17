// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Metalama.Framework.Engine.Utilities
{
    /// <summary>
    /// Exposes the <see cref="AttachDebugger"/> method.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class DebuggingHelper
    {
        private static readonly object _sync = new();
        private static readonly ConditionalWeakTable<object, ObjectId> _objectIds = new();
        private static volatile bool _attachDebuggerRequested;

        public static ProcessKind ProcessKind
            => Process.GetCurrentProcess().ProcessName.ToLowerInvariant() switch
            {
                "devenv" => ProcessKind.DevEnv,
                "servicehub.roslyncodeanalysisservice" => ProcessKind.RoslynCodeAnalysisService,
                "csc" => ProcessKind.Compiler,
                "dotnet" =>
                    Environment.CommandLine.Contains( "JetBrains.ReSharper.Roslyn.Worker.exe" ) ? ProcessKind.Rider :
                    Environment.CommandLine.Contains( "VBCSCompiler.dll" ) ? ProcessKind.Compiler :
                    ProcessKind.Other,
                _ => ProcessKind.Other
            };

        public static void RequireMetalamaCompiler()
        {
            if ( ProcessKind == ProcessKind.Compiler && !MetalamaCompilerInfo.IsActive )
            {
                throw new AssertionFailedException( "Metalama is running in the vanilla C# compiler instead of the customized one." );
            }
        }

        private static int GetObjectIdImpl( object o ) => _objectIds.GetOrCreateValue( o ).Id;

        // The argument type must be specified explicitly to make sure we are not creating ids for unwanted objects.
        // This avoids e.g. confusion between PartialCompilation and Compilation.
        public static int GetObjectId( Compilation o ) => GetObjectIdImpl( o );

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
                    ProcessKind.Rider => projectOptions.DebugAnalyzerProcess,
                    ProcessKind.Compiler => projectOptions.DebugCompilerProcess,
                    _ => false
                };

            if ( mustAttachDebugger )
            {
                lock ( _sync )
                {
                    if ( !_attachDebuggerRequested )
                    {
                        // We try to request to attach the debugger a single time, even if the user refuses or if the debugger gets
                        // detached. It makes a better debugging experience.
                        _attachDebuggerRequested = true;

                        if ( !Debugger.IsAttached )
                        {
                            Debugger.Launch();
                        }
                    }
                }
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class ObjectId
        {
            public int Id { get; }

            private static int _nextId;

            public ObjectId()
            {
                this.Id = Interlocked.Increment( ref _nextId );
            }
        }
    }
}