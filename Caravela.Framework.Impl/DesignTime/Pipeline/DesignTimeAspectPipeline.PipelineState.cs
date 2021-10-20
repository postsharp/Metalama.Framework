// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.DesignTime.Diagnostics;
using Caravela.Framework.Impl.DesignTime.Diff;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Options;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace Caravela.Framework.Impl.DesignTime.Pipeline
{
    internal partial class DesignTimeAspectPipeline
    {
        internal readonly struct PipelineState
        {
            private readonly DesignTimeAspectPipeline _pipeline;

            private readonly CompilationChangeTracker _compilationChangeTracker;

            private static readonly ImmutableDictionary<string, SyntaxTree?> _emptyCompileTimeSyntaxTrees =
                ImmutableDictionary.Create<string, SyntaxTree?>( StringComparer.Ordinal );

            // Syntax trees that may have compile time code based on namespaces. When a syntax tree is known to be compile-time but
            // has been invalidated, we don't remove it from the dictionary, but we set its value to null. It allows to know
            // that this specific tree is outdated, which then allows us to display a warning.

            public ImmutableDictionary<string, SyntaxTree?>? CompileTimeSyntaxTrees { get; }

            public AspectProjectConfiguration? Configuration { get; }

            internal DesignTimeAspectPipelineStatus Status { get; }

            public CompilationChanges? UnprocessedChanges => this._compilationChangeTracker.UnprocessedChanges;

            public CompilationResult CompilationResult { get; }

            internal PipelineState( DesignTimeAspectPipeline pipeline ) : this()
            {
                this._pipeline = pipeline;
                this.CompilationResult = new CompilationResult();
            }

            private PipelineState( PipelineState prototype )
            {
                this._pipeline = prototype._pipeline;
                this._compilationChangeTracker = prototype._compilationChangeTracker;
                this.CompileTimeSyntaxTrees = prototype.CompileTimeSyntaxTrees;
                this.Configuration = prototype.Configuration;
                this.Status = prototype.Status;
                this.CompilationResult = prototype.CompilationResult;
            }

            private PipelineState(
                PipelineState prototype,
                AspectProjectConfiguration configuration,
                DesignTimeAspectPipelineStatus status ) : this( prototype )
            {
                this.Configuration = configuration;
                this.Status = status;
            }

            private PipelineState(
                PipelineState prototype,
                ImmutableDictionary<string, SyntaxTree?> compileTimeSyntaxTrees,
                DesignTimeAspectPipelineStatus status,
                CompilationChangeTracker tracker,
                CompilationResult compilationResult )
                : this( prototype )
            {
                this.CompileTimeSyntaxTrees = compileTimeSyntaxTrees;
                this.Status = status;
                this._compilationChangeTracker = tracker;
                this.CompilationResult = compilationResult;
            }

            private PipelineState( PipelineState prototype, ImmutableDictionary<string, SyntaxTree?> compileTimeSyntaxTrees )
                : this( prototype )
            {
                this.CompileTimeSyntaxTrees = compileTimeSyntaxTrees;
            }

            private PipelineState( PipelineState prototype, CompilationChangeTracker tracker, CompilationResult compilationResult ) : this( prototype )
            {
                this.CompilationResult = compilationResult;
                this._compilationChangeTracker = tracker;
            }

            private static IReadOnlyList<SyntaxTree> GetCompileTimeSyntaxTrees(
                ref PipelineState state,
                Compilation compilation,
                CancellationToken cancellationToken )
            {
                List<SyntaxTree> trees = new( state.CompileTimeSyntaxTrees?.Count ?? 8 );

                if ( state.CompileTimeSyntaxTrees == null )
                {
                    // The cache has not been set yet, so we need to compute the value from zero.

                    if ( state._compilationChangeTracker.LastCompilation != null && state._compilationChangeTracker.LastCompilation != compilation )
                    {
                        throw new AssertionFailedException();
                    }

                    var newCompileTimeSyntaxTrees = ImmutableDictionary<string, SyntaxTree?>.Empty;

                    foreach ( var syntaxTree in compilation.SyntaxTrees )
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if ( CompileTimeCodeDetector.HasCompileTimeCode( syntaxTree.GetRoot() ) )
                        {
                            newCompileTimeSyntaxTrees = newCompileTimeSyntaxTrees.Add( syntaxTree.FilePath, syntaxTree );
                            trees.Add( syntaxTree );
                        }
                    }

                    state = new PipelineState( state, newCompileTimeSyntaxTrees );
                }
                else
                {
                    foreach ( var syntaxTree in compilation.SyntaxTrees )
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if ( state.CompileTimeSyntaxTrees.ContainsKey( syntaxTree.FilePath ) )
                        {
                            trees.Add( syntaxTree );
                        }
                    }
                }

                return trees;
            }

            public PipelineState Reset() => new( this );

            /// <summary>
            /// Invalidates the cache given a new <see cref="Compilation"/>.
            /// </summary>
            internal PipelineState InvalidateCacheForNewCompilation(
                Compilation newCompilation,
                bool invalidateCompilationResult,
                CancellationToken cancellationToken )
            {
                var newTracker = this._compilationChangeTracker.Update( newCompilation, cancellationToken );
                var newStatus = this.Status;
                var newState = this;

                var newChanges = newTracker.UnprocessedChanges.AssertNotNull();

                ImmutableDictionary<string, SyntaxTree?> newCompileTimeSyntaxTrees;

                if ( newChanges.HasCompileTimeCodeChange )
                {
                    if ( this.CompileTimeSyntaxTrees == null && newChanges.IsIncremental )
                    {
                        throw new AssertionFailedException( "Got an incremental compilation change, but _compileTimeSyntaxTrees is null." );
                    }

                    var compileTimeSyntaxTreesBuilder = this.CompileTimeSyntaxTrees?.ToBuilder()
                                                        ?? ImmutableDictionary.CreateBuilder<string, SyntaxTree?>( StringComparer.Ordinal );

                    foreach ( var change in newChanges.SyntaxTreeChanges )
                    {
                        switch ( change.CompileTimeChangeKind )
                        {
                            case CompileTimeChangeKind.None:
                                if ( change.HasCompileTimeCode )
                                {
                                    // When a syntax tree is known to be compile-time but has been invalidated, we don't remove it from the dictionary,
                                    // but we set its value to null. It allows to know that this specific tree is outdated,
                                    // which then allows us to display a warning.
                                    compileTimeSyntaxTreesBuilder[change.FilePath] = null;

                                    // We require an external build because we don't want to invalidate the pipeline configuration at
                                    // each keystroke.
                                    OnCompileTimeChange();
                                }

                                break;

                            case CompileTimeChangeKind.NewlyCompileTime:
                                compileTimeSyntaxTreesBuilder[change.FilePath] = change.NewTree;
                                OnCompileTimeChange();

                                break;

                            case CompileTimeChangeKind.NoLongerCompileTime:
                                compileTimeSyntaxTreesBuilder.Remove( change.FilePath );
                                OnCompileTimeChange();

                                break;
                        }
                    }

                    newCompileTimeSyntaxTrees = compileTimeSyntaxTreesBuilder.ToImmutable();
                }
                else
                {
                    newCompileTimeSyntaxTrees = this.CompileTimeSyntaxTrees ?? _emptyCompileTimeSyntaxTrees;
                }

                var newCompilationResult = invalidateCompilationResult ? this.CompilationResult.Invalidate( newChanges ) : this.CompilationResult;

                newState = new PipelineState(
                    newState,
                    newCompileTimeSyntaxTrees,
                    newStatus,
                    newTracker,
                    newCompilationResult );

                return newState;

                void OnCompileTimeChange()
                {
                    invalidateCompilationResult = false;

                    if ( newState.Status == DesignTimeAspectPipelineStatus.Ready )
                    {
                        Logger.Instance?.Write( $"DesignTimeAspectPipeline.InvalidateCache('{newCompilation.AssemblyName}'): compile-time change detected." );

                        var pipeline = newState._pipeline;

                        newStatus = DesignTimeAspectPipelineStatus.NeedsExternalBuild;

                        if ( pipeline.ProjectOptions.BuildTouchFile != null && File.Exists( pipeline.ProjectOptions.BuildTouchFile ) )
                        {
                            using var mutex = MutexHelper.WithGlobalLock( pipeline.ProjectOptions.BuildTouchFile );
                            File.Delete( pipeline.ProjectOptions.BuildTouchFile );
                        }
                    }
                }
            }

            internal static bool TryGetConfiguration(
                ref PipelineState state,
                PartialCompilation compilation,
                IDiagnosticAdder diagnosticAdder,
                bool ignoreStatus,
                CancellationToken cancellationToken,
                [NotNullWhen( true )] out AspectProjectConfiguration? configuration )
            {
                if ( state.Status == DesignTimeAspectPipelineStatus.NeedsExternalBuild && ignoreStatus )
                {
                    state = new PipelineState( state._pipeline );
                }

                if ( state.Configuration == null )
                {
                    // If we don't have any configuration, we will build one, because this is the first time we are called.

                    var compileTimeTrees = GetCompileTimeSyntaxTrees( ref state, compilation.Compilation, cancellationToken );

                    if ( !state._pipeline.TryInitialize(
                        diagnosticAdder,
                        compilation,
                        compileTimeTrees,
                        cancellationToken,
                        out configuration ) )
                    {
                        // A failure here means an error or a cache miss.

                        Logger.Instance?.Write( $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}') failed." );

                        configuration = null;

                        return false;
                    }
                    else
                    {
                        Logger.Instance?.Write(
                            $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}') succeeded with a new configuration." );

                        state = new PipelineState( state, configuration, DesignTimeAspectPipelineStatus.Ready );

                        return true;
                    }
                }
                else
                {
                    if ( state.Status == DesignTimeAspectPipelineStatus.NeedsExternalBuild )
                    {
                        configuration = null;

                        return false;
                    }

                    // We have a valid configuration and it is not outdated.

                    Logger.Instance?.Write(
                        $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}') returned existing configuration." );

                    configuration = state.Configuration;

                    return true;
                }
            }

            /// <summary>
            /// Executes the pipeline.
            /// </summary>
            public static bool TryExecute(
                ref PipelineState state,
                PartialCompilation compilation,
                CancellationToken cancellationToken,
                [NotNullWhen( true )] out CompilationResult? compilationResult )
            {
                DiagnosticList diagnosticList = new();

                if ( state.Status == DesignTimeAspectPipelineStatus.NeedsExternalBuild )
                {
                    throw new InvalidOperationException();
                }

                if ( !TryGetConfiguration( ref state, compilation, diagnosticList, false, cancellationToken, out var configuration ) )
                {
                    compilationResult = null;

                    return false;
                }

                var success = state._pipeline.TryExecute( compilation, diagnosticList, configuration, cancellationToken, out var pipelineResult );

                var result = new DesignTimeAspectPipelineResult(
                    success,
                    compilation.SyntaxTrees,
                    pipelineResult?.AdditionalSyntaxTrees ?? Array.Empty<IntroducedSyntaxTree>(),
                    new ImmutableUserDiagnosticList(
                        diagnosticList.ToImmutableArray(),
                        pipelineResult?.Diagnostics.DiagnosticSuppressions ),
                    pipelineResult?.ExternallyInheritableAspects ?? ImmutableArray<AttributeAspectInstance>.Empty );

                var directoryOptions = state._pipeline.ServiceProvider.GetService<IPathOptions>();
                UserDiagnosticRegistrationService.GetInstance( directoryOptions ).RegisterDescriptors( result );

                state = state.SetResult( compilation, result );
                compilationResult = state.CompilationResult;

                return true;
            }

            private PipelineState SetResult( PartialCompilation compilation, DesignTimeAspectPipelineResult pipelineResult )
            {
                var compilationResult = this.CompilationResult.Update( compilation, pipelineResult );

                return new PipelineState( this, this._compilationChangeTracker.ResetUnprocessedChanges(), compilationResult );
            }
        }
    }
}