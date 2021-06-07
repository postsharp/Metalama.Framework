// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Eligibility;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.DesignTime.Refactoring;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.Pipeline
{
    /// <summary>
    /// Caches the <see cref="DesignTimeAspectPipeline"/> (so they can be reused between projects) and the
    /// returns produced by <see cref="DesignTimeAspectPipeline"/>. This class is also responsible for invoking
    /// cache invalidation methods as appropriate.
    /// </summary>
    internal class DesignTimeAspectPipelineCache : IDisposable
    {
        private static readonly string _sourceGeneratorAssemblyName = typeof(DesignTimeAspectPipelineCache).Assembly.GetName().Name;

        private readonly ConditionalWeakTable<Compilation, object> _sync = new();
        private readonly ConcurrentDictionary<string, DesignTimeAspectPipeline> _pipelinesByProjectId = new();
        private readonly SyntaxTreeResultCache _syntaxTreeResultCache = new();
        private readonly CompileTimeDomain _domain;
        private int _pipelineExecutionCount;

        public DesignTimeAspectPipelineCache( CompileTimeDomain domain )
        {
            this._domain = domain;
        }

        /// <summary>
        /// Gets the singleton instance of this class (other instances can be used in tests).
        /// </summary>
        public static DesignTimeAspectPipelineCache Instance { get; } = new( new CompileTimeDomain() );

        /// <summary>
        /// Gets the number of times the pipeline has been executed. Useful for testing purposes.
        /// </summary>
        public int PipelineExecutionCount => this._pipelineExecutionCount;

        /// <summary>
        /// Gets the pipeline for a given project, and creates it if necessary.
        /// </summary>
        /// <param name="projectOptions"></param>
        /// <returns></returns>
        internal DesignTimeAspectPipeline GetOrCreatePipeline( IProjectOptions projectOptions )
            => this._pipelinesByProjectId.GetOrAdd(
                projectOptions.ProjectId,
                _ =>
                {
                    var pipeline = new DesignTimeAspectPipeline( projectOptions, this._domain );
                    pipeline.ExternalBuildStarted += this.OnExternalBuildStarted;

                    return pipeline;
                } );

        private void OnExternalBuildStarted( object sender, EventArgs e )
        {
            // If a build has started, we have to invalidate the whole cache because we have allowed
            // our cache to become inconsistent when we started to have an outdated pipeline configuration.
            this._syntaxTreeResultCache.Clear();
        }

        public IEnumerable<AspectClass> GetEligibleAspects( ISymbol symbol, IProjectOptions projectOptions, CancellationToken cancellationToken )
        {
            var pipeline = this.GetOrCreatePipeline( projectOptions );

            var classes = pipeline.AspectClasses;

            if ( classes != null )
            {
                foreach ( var aspectClass in classes )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if ( aspectClass.IsEligible( symbol, EligibilityValue.EligibleForInheritanceOnly ) )
                    {
                        yield return aspectClass;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the design-time results for a whole compilation.
        /// </summary>
        public ImmutableArray<SyntaxTreeResult> GetSyntaxTreeResults(
            Compilation compilation,
            IProjectOptions projectOptions,
            CancellationToken cancellationToken = default )
            => this.GetSyntaxTreeResults( compilation, compilation.SyntaxTrees.ToImmutableArray(), projectOptions, cancellationToken );

        /// <summary>
        /// Gets the design-time results for a set of syntax trees.
        /// </summary>
        public ImmutableArray<SyntaxTreeResult> GetSyntaxTreeResults(
            Compilation compilation,
            IReadOnlyList<SyntaxTree> syntaxTrees,
            IProjectOptions projectOptions,
            CancellationToken cancellationToken )
        {
            var pipeline = this.GetOrCreatePipeline( projectOptions );

            // Update the cache.
            var changes = pipeline.InvalidateCache( compilation );

            if ( pipeline.Status == DesignTimeAspectPipelineStatus.Ready )
            {
                this._syntaxTreeResultCache.InvalidateCache( changes );
            }
            else
            {
                // We don't invalidate the syntax tree results cache if the pipeline is broken, so we can serve old (outdated) results
                // instead of nothing.
            }

            if ( pipeline.Status != DesignTimeAspectPipelineStatus.NeedsExternalBuild )
            {
                var dirtySyntaxTrees = this.GetDirtySyntaxTrees( compilation );

                // Execute the pipeline if required, and update the cache.
                if ( dirtySyntaxTrees.Count > 0 )
                {
                    var lockable = this._sync.GetOrCreateValue( compilation );

                    lock ( lockable )
                    {
                        var partialCompilation = PartialCompilation.CreatePartial( compilation, dirtySyntaxTrees );

                        if ( !partialCompilation.IsEmpty )
                        {
                            Interlocked.Increment( ref this._pipelineExecutionCount );
                            var result = pipeline.Execute( partialCompilation, cancellationToken );

                            this._syntaxTreeResultCache.SetResults( compilation, result );
                        }
                    }
                }
            }
            else
            {
                // If we need a build, we only serve results from the cache.
                Logger.Instance?.Write(
                    $"DesignTimeAspectPipelineCache.GetDesignTimeResults('{compilation.AssemblyName}'): build required," +
                    $" returning from cache only. Cache size is {this._syntaxTreeResultCache.Count}" );
            }

            // Get the results from the cache. We don't need to check dependencies
            var resultArrayBuilder = ImmutableArray.CreateBuilder<SyntaxTreeResult>( syntaxTrees.Count );

            // Create the result.
            foreach ( var syntaxTree in syntaxTrees )
            {
                if ( this._syntaxTreeResultCache.TryGetResult( syntaxTree, out var syntaxTreeResult ) )
                {
                    resultArrayBuilder.Add( syntaxTreeResult );
                }
            }

            return resultArrayBuilder.ToImmutable();
        }

        private List<SyntaxTree> GetDirtySyntaxTrees( Compilation compilation )
        {
            // Computes the set of semantic models that need to be processed.

            List<SyntaxTree> uncachedSyntaxTrees = new();

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                if ( syntaxTree.FilePath.StartsWith( _sourceGeneratorAssemblyName, StringComparison.Ordinal ) )
                {
                    // This is our own generated file. Don't include.
                    continue;
                }

                if ( !this._syntaxTreeResultCache.TryGetResult( syntaxTree, out _ ) )
                {
                    uncachedSyntaxTrees.Add( syntaxTree );
                }
            }

            return uncachedSyntaxTrees;
        }

        public bool TryApplyAspectToCode(
            IProjectOptions projectOptions,
            AspectClass aspectClass,
            Compilation inputCompilation,
            ISymbol targetSymbol,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out Compilation? outputCompilation )
        {
            var designTimePipeline = this.GetOrCreatePipeline( projectOptions );

            // TODO: use partial compilation (it does not seem to work).
            var partialCompilation = PartialCompilation.CreateComplete( inputCompilation );

            if ( !designTimePipeline.TryGetConfiguration( partialCompilation, NullDiagnosticAdder.Instance, cancellationToken, out var configuration ) )
            {
                outputCompilation = null;

                return false;
            }

            return ApplyToSourceCodeAspectPipeline.TryExecute(
                projectOptions,
                this._domain,
                configuration,
                aspectClass,
                partialCompilation,
                targetSymbol,
                cancellationToken,
                out outputCompilation );
        }

        public void Dispose() => this._domain.Dispose();
    }
}