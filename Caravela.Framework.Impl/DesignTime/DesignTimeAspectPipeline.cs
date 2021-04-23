// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// The main entry point of Caravela when called from a Roslyn source generator.
    /// </summary>
    internal class DesignTimeAspectPipeline : AspectPipeline
    {
        private readonly object configureSync = new();
        private PipelineConfiguration? _cachedConfiguration;
        
        // Maps file names to source text on which we have a dependency.
        private ConcurrentDictionary<string, SyntaxTree> _cacheDependencies = new();
        public DesignTimeAspectPipeline( IBuildOptions buildOptions ) : base( buildOptions ) { }

        public void OnSemanticModelUpdated( SemanticModel semanticModel )
        {
            if ( this._cachedConfiguration == null )
            {
                // The cache is empty, so there is nothing to invalidate.
                return;
            }
            
            var newSyntaxTree = semanticModel.SyntaxTree;

            if ( this._cacheDependencies.TryGetValue( newSyntaxTree.FilePath, out var oldSyntaxTree ) )
            {
                if ( !newSyntaxTree.IsEquivalentTo( oldSyntaxTree ) )
                {
                    // We have a breaking change in our pipeline configuration.
                    this._cachedConfiguration = null;
                }
            }
        }

        private bool TryGetConfiguration( CompilationModel compilationModel, IDiagnosticAdder diagnosticAdder,  [NotNullWhen(true)] out PipelineConfiguration? configuration )
        {
            // Build the pipeline configuration if we don't have a valid one.
            if ( this._cachedConfiguration == null )
            {
                lock ( this.configureSync )
                {
                    if ( this._cachedConfiguration == null )
                    {
                        if ( !this.Initialize(
                            diagnosticAdder,
                            compilationModel,
                            ImmutableArray<object>.Empty,
                            out configuration ) )
                        {
                            configuration = null;
                            return false;
                        }

                        // Update cache dependencies.
                        this._cacheDependencies.Clear();

                        if ( configuration.CompileTimeProject != null )
                        {
                            foreach ( var syntaxTree in configuration.CompileTimeProject.SyntaxTrees )
                            {
                                this._cacheDependencies.TryAdd( syntaxTree.FilePath, syntaxTree );
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


        public DesignTimeAspectPipelineResult AnalyzeCompilation( Compilation compilation ) 
        {
            
            var compilationModel = CompilationModel.CreateInitialInstance( compilation );


            return this.ExecuteCore( compilationModel );
        }
        
        
        
        public DesignTimeAspectPipelineResult AnalyzeSemanticModel( SemanticModel semanticModel )
        {
            var compilationModel = PartialCompilationModel.CreateInitialInstance( semanticModel );
                
            return this.ExecuteCore( compilationModel );
        }

        private DesignTimeAspectPipelineResult ExecuteCore( CompilationModel compilation )
        {
            DiagnosticList diagnosticList = new();
            
            if ( !this.TryGetConfiguration( compilation, diagnosticList, out var configuration ) )
            {
                return new DesignTimeAspectPipelineResult(
                    false, null, new ImmutableDiagnosticList( diagnosticList ) );
            }
            
            var success = this.TryExecuteCore( compilation, diagnosticList, configuration, out var pipelineResult );

            return new DesignTimeAspectPipelineResult(
                success,
                pipelineResult?.AdditionalSyntaxTrees,
                new ImmutableDiagnosticList(
                    diagnosticList.ToImmutableArray(),
                    pipelineResult?.Diagnostics.DiagnosticSuppressions ) );
        }

        public override bool CanTransformCompilation => false;

        /// <inheritdoc/>
        private protected override HighLevelPipelineStage CreateStage(
            IReadOnlyList<OrderedAspectLayer> parts,
            CompileTimeAssemblyLoader compileTimeAssemblyLoader )
            => new SourceGeneratorPipelineStage( parts, this );
    }
}