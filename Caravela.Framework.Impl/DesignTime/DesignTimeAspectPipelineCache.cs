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
    internal record DesignTimeResults ( ImmutableArray<DesignTimeSyntaxTreeResult> SyntaxTreeResults );
    
    internal partial class DesignTimeAspectPipelineCache
    {
        private readonly ConditionalWeakTable<Compilation, object> _sync = new();
        private readonly ConcurrentDictionary<string, DesignTimeAspectPipeline> _pipelinesByProjectId = new();
        private readonly DesignTimeSyntaxTreeResultCache _syntaxTreeResultCache = new();
        private bool _attachDebuggerRequested;

        public static DesignTimeAspectPipelineCache Instance { get; } = new();
    

        private void AttachDebugger( BuildOptions buildOptions )
        {
            if ( buildOptions.DesignTimeAttachDebugger && !this._attachDebuggerRequested )
            {
                // We try to request to attach the debugger a single time, even if the user refuses or if the debugger gets
                // detaches. It makes a better debugging experience.
                this._attachDebuggerRequested = true;

                if ( !Process.GetCurrentProcess().ProcessName.Equals( "devenv", StringComparison.OrdinalIgnoreCase ) &&
                     !Debugger.IsAttached )
                {
                    Debugger.Launch();
                }
            }
        }

        private DesignTimeAspectPipeline GetOrCreatePipeline( BuildOptions buildOptions )
            => this._pipelinesByProjectId.GetOrAdd( buildOptions.ProjectId, _ => new DesignTimeAspectPipeline( buildOptions ) );

        public DesignTimeResults GetDesignTimeResults(
            Compilation compilation,
            BuildOptions buildOptions,
            CancellationToken cancellationToken )
            => this.GetDesignTimeResults( compilation, compilation.SyntaxTrees.ToImmutableArray(), buildOptions, cancellationToken );

        public DesignTimeResults GetDesignTimeResults(
            Compilation compilation,
            IReadOnlyList<SyntaxTree> syntaxTrees,
            BuildOptions buildOptions,
            CancellationToken cancellationToken )
        {
            this.AttachDebugger( buildOptions );

            var pipeline = this.GetOrCreatePipeline( buildOptions );

            // Invalidate the cache, if required.
            foreach ( var syntaxTree in syntaxTrees )
            {
                pipeline.OnSyntaxTreePossiblyChanged( syntaxTree, out var configurationInvalidated );

                if ( configurationInvalidated )
                {
                    this._syntaxTreeResultCache.Clear();

                    break;
                }

                this._syntaxTreeResultCache.OnSyntaxTreePossiblyChanged( syntaxTree );
            }

            // Computes the set of semantic models that need to be processed.
            List<SyntaxTree> uncachedSyntaxTrees = new();

            foreach ( var syntaxTree in syntaxTrees )
            {
                if ( !this._syntaxTreeResultCache.TryGetValue( syntaxTree, out _, true ) )
                {
                    uncachedSyntaxTrees.Add( syntaxTree );
                }
            }

            // Execute the pipeline if required, and update the cache.
            if ( uncachedSyntaxTrees.Count > 0 )
            {
                var lockable = this._sync.GetOrCreateValue( compilation );

                lock ( lockable )
                {
                    var partialCompilation = PartialCompilation.CreatePartial( compilation, uncachedSyntaxTrees );
                    var result = pipeline.Execute( partialCompilation );

                    this._syntaxTreeResultCache.Update( compilation, result );
                }
            }

            // Get the results from the cache. We don't need to check dependencies
            var resultArrayBuilder = ImmutableArray.CreateBuilder<DesignTimeSyntaxTreeResult>( syntaxTrees.Count );

            foreach ( var syntaxTree in syntaxTrees )
            {
                // Get the result from the cache, but there is no need to validate dependencies because we've just dont it an
                // instant ago and a data race and it is ok if the data race is won by the competing task.

                if ( this._syntaxTreeResultCache.TryGetValue( syntaxTree, out var syntaxTreeResult, false ) )
                {
                    resultArrayBuilder.Add( syntaxTreeResult );
                }
            }

            return new DesignTimeResults( resultArrayBuilder.ToImmutable() );
        }
    }
}