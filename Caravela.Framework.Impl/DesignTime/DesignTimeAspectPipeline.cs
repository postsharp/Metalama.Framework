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

namespace Caravela.Framework.Impl.DesignTime
{
    internal enum DesignTimeAspectPipelineStatus
    {
        /// <summary>
        /// The pipeline has never been successfully initialized.
        /// </summary>
        Default,

        /// <summary>
        /// The pipeline has a working configuration.
        /// </summary>
        Ready,

        /// <summary>
        /// The pipeline configuration is outdated. A project build is required.
        /// </summary>
        NeedsExternalBuild
    }

    /// <summary>
    /// The design-time implementation of <see cref="AspectPipeline"/>.
    /// </summary>
    internal class DesignTimeAspectPipeline : AspectPipeline
    {
        // Syntax trees that may have compile time code based on namespaces.
        private readonly ConcurrentDictionary<string, SyntaxTree?> _compileTimeSyntaxTrees = new();

        private readonly object _configureSync = new();
        private bool _compileTimeSyntaxTreesInitialized;
        private PipelineConfiguration? _lastKnownConfiguration;

        public DesignTimeAspectPipelineStatus Status { get; private set; }

        // TODO: The cache must invalidate itself on build (e.g. when the output directory content changes)

        public IReadOnlyList<SyntaxTree> GetCompileTimeSyntaxTrees( Compilation compilation )
        {
            List<SyntaxTree> trees = new( this._compileTimeSyntaxTrees.Count );

            if ( !this._compileTimeSyntaxTreesInitialized )
            {
                this._compileTimeSyntaxTrees.Clear();
                
                foreach ( var syntaxTree in compilation.SyntaxTrees )
                {
                    if ( CompileTimeCodeDetector.HasCompileTimeCode( syntaxTree.GetRoot() ) )
                    {
                        this._compileTimeSyntaxTrees.TryAdd( syntaxTree.FilePath, syntaxTree );
                        trees.Add( syntaxTree );
                    }
                }

                this._compileTimeSyntaxTreesInitialized = true;
            }
            else
            {
                foreach ( var syntaxTree in compilation.SyntaxTrees )
                {
                    if ( this._compileTimeSyntaxTrees.ContainsKey( syntaxTree.FilePath ) )
                    {
                        trees.Add( syntaxTree );
                    }
                }
            }

            return trees;
        }

        public DesignTimeAspectPipeline( IBuildOptions buildOptions, CompileTimeDomain domain ) : base( buildOptions, domain ) { }

        public bool IsCompileTimeSyntaxTreeOutdated( string name )
            => this._compileTimeSyntaxTrees.TryGetValue( name, out var syntaxTree ) && syntaxTree == null;
        
        public bool InvalidateCache( SyntaxTree syntaxTree, bool hasCompileTimeCode )
        {
            if ( this._lastKnownConfiguration == null )
            {
                // The cache is empty, so there is nothing to invalidate.
                return false;
            }

            if ( this._compileTimeSyntaxTrees.TryGetValue( syntaxTree.FilePath, out var oldSyntaxTree ) )
            {
                if ( oldSyntaxTree != null )
                {
                    if ( !hasCompileTimeCode || !syntaxTree.GetText().ContentEquals( oldSyntaxTree.GetText() ) )
                    {
                        // We have a breaking change in our pipeline configuration. 
                        this.Status = DesignTimeAspectPipelineStatus.NeedsExternalBuild;

                        if ( !hasCompileTimeCode )
                        {
                            // The tree no longer contains compile-time code.
                            this._compileTimeSyntaxTrees.TryRemove( syntaxTree.FilePath, out _ );
                        }
                        else
                        {
                            // The tree still contains compile-time code but is outdated.
                            this._compileTimeSyntaxTrees.TryUpdate( syntaxTree.FilePath, null, oldSyntaxTree );
                        }

                        return true;
                    }
                }
                else
                {
                    // The tree was already invalidated before.
                    return false;
                }
            }
            else if ( hasCompileTimeCode )
            {
                // The syntax tree used to have no compile-time code, but now has.
                this._compileTimeSyntaxTrees.TryAdd( syntaxTree.FilePath, syntaxTree );
                this.Status = DesignTimeAspectPipelineStatus.NeedsExternalBuild;

                return true;
            }

            return false;
        }

        public void Reset()
        {
            this._lastKnownConfiguration = null;
            this.Status = DesignTimeAspectPipelineStatus.Default;
            this._compileTimeSyntaxTreesInitialized = false;
        }

        private bool TryGetConfiguration(
            PartialCompilation compilation,
            IDiagnosticAdder diagnosticAdder,
            [NotNullWhen( true )] out PipelineConfiguration? configuration )
        {
            lock ( this._configureSync )
            {
                if ( this._lastKnownConfiguration == null )
                {
                    // If we don't have any configuration, we will build one, because this is the first time we are called.

                    var compileTimeTrees = this.GetCompileTimeSyntaxTrees( compilation.Compilation );

                    if ( !this.TryInitialize(
                        diagnosticAdder,
                        compilation,
                        compileTimeTrees,
                        out configuration ) )
                    {
                        // A failure here means an error or a cache miss.
                        configuration = null;

                        return false;
                    }
                    else
                    {
                        this._lastKnownConfiguration = configuration;
                        this.Status = DesignTimeAspectPipelineStatus.Ready;

                        return true;
                    }
                }
                else
                {
                    if ( this.Status == DesignTimeAspectPipelineStatus.NeedsExternalBuild )
                    {
                        throw new InvalidOperationException();
                    }

                    // We have a valid configuration and it is not outdated.

                    configuration = this._lastKnownConfiguration;

                    return true;
                }
            }
        }

        public DesignTimeAspectPipelineResult Execute( PartialCompilation compilation )
        {
            DiagnosticList diagnosticList = new();

            if ( this.Status == DesignTimeAspectPipelineStatus.NeedsExternalBuild )
            {
                throw new InvalidOperationException();
            }

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