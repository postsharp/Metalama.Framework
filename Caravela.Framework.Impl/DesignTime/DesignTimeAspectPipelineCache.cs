// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime
{
    internal static partial class DesignTimeAspectPipelineCache
    {
        private static readonly ConditionalWeakTable<object, object> _sync = new();
        private static readonly ConcurrentDictionary<string, DesignTimeAspectPipeline> _pipelineCache = new();
        private static bool _attachDebuggerRequested;

        private static void AttachDebugger( BuildOptions buildOptions )
        {
            if ( buildOptions.DesignTimeAttachDebugger && !_attachDebuggerRequested )
            {
                // We try to request to attach the debugger a single time, even if the user refuses or if the debugger gets
                // detaches. It makes a better debugging experience.
                _attachDebuggerRequested = true;

                if ( !Process.GetCurrentProcess().ProcessName.Equals( "devenv", StringComparison.OrdinalIgnoreCase ) &&
                     !Debugger.IsAttached )
                {
                    Debugger.Launch();
                }
            }
        }


        private static DesignTimeAspectPipeline GetOrCreatePipeline( BuildOptions buildOptions )
            => _pipelineCache.GetOrAdd( buildOptions.ProjectId, _ => new DesignTimeAspectPipeline( buildOptions ) );


        public static DesignTimeAspectPipelineResult GetPipelineResult(
            Compilation compilation,
            BuildOptions buildOptions,
            CancellationToken cancellationToken )
        {
            AttachDebugger( buildOptions );

            // ReSharper disable once InconsistentlySynchronizedField

            if ( !ResultCache.TryGetValue( compilation, out var result ) )
            {
                var lockable = _sync.GetOrCreateValue( compilation );

                lock ( lockable )
                {
                    if ( !ResultCache.TryGetValue( compilation, out result ) )
                    {
                        var pipeline = GetOrCreatePipeline( buildOptions );

                        result = pipeline.Execute( PartialCompilation.CreateComplete( compilation ) );

                        ResultCache.Update( compilation, result );
                    }
                }
            }

            return result;
        }
        
        public static DesignTimeAspectPipelineResult GetPipelineResult(
            SemanticModel semanticModel,
            BuildOptions buildOptions,
            CancellationToken cancellationToken )
        {
            AttachDebugger( buildOptions );

            // ReSharper disable once InconsistentlySynchronizedField

            if ( !ResultCache.TryGetValue( semanticModel, out var result ) )
            {
                var lockable = _sync.GetOrCreateValue( semanticModel );

                lock ( lockable )
                {
                    if ( !ResultCache.TryGetValue( semanticModel, out result ) )
                    {
                        var pipeline = GetOrCreatePipeline( buildOptions );


                        // If Roslyn has requested an analysis of this semantic model, there is a chance that it has changed, therefore we should
                        // consider invalidating the pipeline configuration cache.
                        pipeline.OnSemanticModelUpdated( semanticModel );

                        result = pipeline.Execute( PartialCompilation.CreatePartial( semanticModel ) );

                        ResultCache.Update( semanticModel, result );
                    }
                }
            }

            return result;
        }
    }
}