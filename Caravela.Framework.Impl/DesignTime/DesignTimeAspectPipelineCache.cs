// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime
{
    internal static partial class DesignTimeAspectPipelineCache
    {
        private static readonly ConditionalWeakTable<Compilation, object> _sync = new();
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

        public static ImmutableArray<SyntaxTreeResult> GetPipelineResult(
            Compilation compilation,
            BuildOptions buildOptions,
            CancellationToken cancellationToken )
            => GetPipelineResult( compilation, compilation.SyntaxTrees.ToImmutableArray(), buildOptions, cancellationToken );

        public static ImmutableArray<SyntaxTreeResult> GetPipelineResult(
            Compilation compilation,
            IReadOnlyList<SyntaxTree> syntaxTrees,
            BuildOptions buildOptions,
            CancellationToken cancellationToken )
        {
            AttachDebugger( buildOptions );

            var pipeline = GetOrCreatePipeline( buildOptions );

            // Invalidate the cache, if required.
            foreach ( var syntaxTree in syntaxTrees )
            {
                pipeline.OnSyntaxTreePossiblyChanged( syntaxTree, out var configurationInvalidated );

                if ( configurationInvalidated )
                {
                    SyntaxTreeResultCache.Clear();

                    break;
                }
                    
                SyntaxTreeResultCache.OnSyntaxTreePossiblyChanged( syntaxTree );
            }

            // Computes the set of semantic models that need to be processed.
            List<SyntaxTree> uncachedSyntaxTrees = new();

            foreach ( var syntaxTree in syntaxTrees )
            {
                if ( !SyntaxTreeResultCache.TryGetValue( syntaxTree, out _, true ) )
                {
                    uncachedSyntaxTrees.Add( syntaxTree );
                }
            }

            // Execute the pipeline if required, and update the cache.
            if ( uncachedSyntaxTrees.Count > 0 )
            {
                var lockable = _sync.GetOrCreateValue( compilation );

                lock ( lockable )
                {
                    var partialCompilation = PartialCompilation.CreatePartial( compilation, uncachedSyntaxTrees );
                    var result = pipeline.Execute( partialCompilation );

                    SyntaxTreeResultCache.Update( compilation, result );
                }
            }

            // Get the results from the cache. We don't need to check dependencies
            var resultArrayBuilder = ImmutableArray.CreateBuilder<SyntaxTreeResult>( syntaxTrees.Count );

            foreach ( var syntaxTree in syntaxTrees )
            {
                // Get the result from the cache, but there is no need to validate dependencies because we've just dont it an
                // instant ago and a data race and it is ok if the data race is won by the competing task.
                
                if ( SyntaxTreeResultCache.TryGetValue( syntaxTree, out var syntaxTreeResult, false ) )
                {
                    resultArrayBuilder.Add( syntaxTreeResult );
                }
            }

            return resultArrayBuilder.ToImmutable();
        }
    }
}