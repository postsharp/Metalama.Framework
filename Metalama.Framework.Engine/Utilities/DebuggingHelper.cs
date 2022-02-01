// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Compiler;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Metalama.Framework.Engine.Utilities
{
    [ExcludeFromCodeCoverage]
    internal static class DebuggingHelper
    {
        private static readonly ConditionalWeakTable<object, ObjectId> _objectIds = new();

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

        public static int GetObjectId( AspectPipelineConfiguration o ) => GetObjectIdImpl( o );

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