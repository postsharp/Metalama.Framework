// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Sdk;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime
{
    internal static class DesignTimeAspectPipelineCache
    {
        private static readonly ConditionalWeakTable<object, DesignTimeAspectPipelineResult> _cache = new();
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

        private static void UpdateCache( object key, DesignTimeAspectPipelineResult value )
        {
            if ( _cache.TryGetValue( key, out var currentValue ) && currentValue == value)
            {
                return;
            }

            while ( true )
            {
                _cache.Remove( key );

                try
                {
                    _cache.Add( key, value );
                    return;
                }
                catch ( ArgumentException )
                {
                    
                }
            }
        }

        private static DesignTimeAspectPipeline GetOrCreatePipeline( BuildOptions buildOptions )
            => _pipelineCache.GetOrAdd( buildOptions.ProjectId, _ => new DesignTimeAspectPipeline( buildOptions ) );

        public static DesignTimeAspectPipelineResult GetPipelineResult(
            PartialCompilation compilation,
            AnalyzerBuildOptionsSource buildOptionsSource,
            ImmutableArray<object> plugIns,
            CancellationToken cancellationToken )
            => GetPipelineResult( compilation, new BuildOptions( buildOptionsSource, plugIns ), cancellationToken );

        public static DesignTimeAspectPipelineResult GetPipelineResult(
            PartialCompilation compilation,
            BuildOptions buildOptions,
            CancellationToken cancellationToken )
        {
            AttachDebugger( buildOptions );

            // ReSharper disable once InconsistentlySynchronizedField

            if ( !_cache.TryGetValue( compilation, out var result ) )
            {
                var lockable = _sync.GetOrCreateValue( compilation );

                lock ( lockable )
                {
                    if ( !_cache.TryGetValue( compilation, out result ) )
                    {
                        var pipeline = GetOrCreatePipeline( buildOptions );

                        result = pipeline.Execute( compilation );

                        // Add the result to the cache for all semantic models.
                        foreach ( var syntaxTree in compilation.SyntaxTrees )
                        {
                            var semanticModel = compilation.Compilation.GetSemanticModel( syntaxTree );
                            _ = _cache.Remove( semanticModel );
                            UpdateCache( semanticModel, result );
                        }

                        
                        UpdateCache( compilation, result );
                    }
                }
            }

            return result;
        }
    }
}