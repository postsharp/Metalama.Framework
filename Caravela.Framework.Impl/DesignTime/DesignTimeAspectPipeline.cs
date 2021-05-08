// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// The design-time implementation of <see cref="AspectPipeline"/>.
    /// </summary>
    internal class DesignTimeAspectPipeline : AspectPipeline
    {
        // Maps file names to source text on which we have a dependency.
        private readonly ConcurrentDictionary<string, SyntaxTree> _configurationCacheDependencies = new();

        private readonly object _configureSync = new();
        private PipelineConfiguration? _cachedConfiguration;

        public DesignTimeAspectPipeline( IBuildOptions buildOptions, CompileTimeDomain domain ) : base( buildOptions, domain ) { }

        public void OnSyntaxTreePossiblyChanged( SyntaxTree syntaxTree, out bool isInvalidated )
        {
            isInvalidated = false;

            if ( this._cachedConfiguration == null )
            {
                // The cache is empty, so there is nothing to invalidate.
                return;
            }

            if ( this._configurationCacheDependencies.TryGetValue( syntaxTree.FilePath, out var oldSyntaxTree ) )
            {
                if ( !syntaxTree.GetText().ContentEquals( oldSyntaxTree.GetText() ) )
                {
                    // We have a breaking change in our pipeline configuration.
                    this._cachedConfiguration = null;
                    isInvalidated = true;
                }
            }
        }

        private bool TryGetConfiguration(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            [NotNullWhen( true )] out PipelineConfiguration? configuration )
        {
            // Build the pipeline configuration if we don't have a valid one.
            if ( this._cachedConfiguration == null )
            {
                lock ( this._configureSync )
                {
                    if ( this._cachedConfiguration == null )
                    {
                        if ( !this.TryInitialize(
                            diagnosticAdder,
                            compilation,
                            out configuration ) )
                        {
                            configuration = null;

                            return false;
                        }

                        // Update cache dependencies.
                        this._configurationCacheDependencies.Clear();

                        if ( configuration.CompileTimeProject?.CodeFiles != null )
                        {
                            foreach ( var sourceFile in configuration.CompileTimeProject.CodeFiles )
                            {
                                // TODO: less computationally intensive algorithm.
                                
                                var syntaxTree =
                                    compilation.Compilation.SyntaxTrees.Single( t => sourceFile.SourceEquals( t ) );

                                // We will have to somehow store the mapping.
                                
                                _ = this._configurationCacheDependencies.TryAdd(syntaxTree.FilePath, syntaxTree );
                            }
                        }

                        this._cachedConfiguration = configuration;

                        return true;
                    }
                }
            }

            configuration = this._cachedConfiguration;

            return true;
        }

        public DesignTimeAspectPipelineResult Execute( PartialCompilation compilation )
        {
            DiagnosticList diagnosticList = new();

            if ( !this.TryGetConfiguration( compilation, diagnosticList, out var configuration ) )
            {
                return new DesignTimeAspectPipelineResult(
                    false,
                    compilation.SyntaxTrees,
                    ImmutableArray<IntroducedSyntaxTree>.Empty,
                    new ImmutableDiagnosticList( diagnosticList ) );
            }

            var success = TryExecuteCore( compilation, diagnosticList, configuration, out var pipelineResult );

            return new DesignTimeAspectPipelineResult(
                success,
                compilation.SyntaxTrees,
                pipelineResult?.AdditionalSyntaxTrees ?? Array.Empty<IntroducedSyntaxTree>(),
                new ImmutableDiagnosticList(
                    diagnosticList.ToImmutableArray(),
                    pipelineResult?.Diagnostics.DiagnosticSuppressions ) );
        }

        public override bool CanTransformCompilation => false;

        /// <inheritdoc/>
        private protected override HighLevelPipelineStage CreateStage(
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeProjectLoader compileTimeProjectLoader )
            => new SourceGeneratorPipelineStage( parts, this );
    }
}