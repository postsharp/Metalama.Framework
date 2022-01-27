// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline
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

            public AspectPipelineConfiguration? Configuration { get; }

            internal DesignTimeAspectPipelineStatus Status { get; }

            public CompilationChanges? UnprocessedChanges => this._compilationChangeTracker.UnprocessedChanges;

            public CompilationPipelineResult PipelineResult { get; }

            public CompilationValidationResult ValidationResult { get; }

            internal PipelineState( DesignTimeAspectPipeline pipeline ) : this()
            {
                this._pipeline = pipeline;
                this.PipelineResult = new CompilationPipelineResult();
                this.ValidationResult = CompilationValidationResult.Empty;
            }

            private PipelineState( PipelineState prototype )
            {
                this._pipeline = prototype._pipeline;
                this._compilationChangeTracker = prototype._compilationChangeTracker;
                this.CompileTimeSyntaxTrees = prototype.CompileTimeSyntaxTrees;
                this.Configuration = prototype.Configuration;
                this.Status = prototype.Status;
                this.PipelineResult = prototype.PipelineResult;
                this.ValidationResult = prototype.ValidationResult;
            }

            private PipelineState(
                PipelineState prototype,
                AspectPipelineConfiguration configuration,
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
                CompilationPipelineResult pipelineResult,
                AspectPipelineConfiguration? configuration )
                : this( prototype )
            {
                this.CompileTimeSyntaxTrees = compileTimeSyntaxTrees;
                this.Status = status;
                this._compilationChangeTracker = tracker;
                this.PipelineResult = pipelineResult;
                this.Configuration = configuration;
            }

            private PipelineState( PipelineState prototype, ImmutableDictionary<string, SyntaxTree?> compileTimeSyntaxTrees )
                : this( prototype )
            {
                this.CompileTimeSyntaxTrees = compileTimeSyntaxTrees;
            }

            private PipelineState( PipelineState prototype, CompilationChangeTracker tracker, CompilationPipelineResult pipelineResult ) : this( prototype )
            {
                this.PipelineResult = pipelineResult;
                this._compilationChangeTracker = tracker;
            }

            private PipelineState( PipelineState prototype, CompilationValidationResult validationResult ) : this( prototype )
            {
                this.ValidationResult = validationResult;
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
                var newStatus = this.Status;
                var newState = this;
                var newConfiguration = this.Configuration;

                // Detect changes in compile-time syntax trees.
                var newTracker = this._compilationChangeTracker.Update( newCompilation, cancellationToken );
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
                                    Logger.DesignTime.Trace?.Log( $"Compile-time change detected: {change.FilePath} has changed." );
                                    OnCompileTimeChange( true );
                                }

                                break;

                            case CompileTimeChangeKind.NewlyCompileTime:
                                // We don't require an external rebuild when a new syntax tree is added because Roslyn does not give us a complete
                                // compilation in the first call in the Visual Studio initializations sequence. Roslyn calls us later with
                                // a complete compilation, but we don't want to bother the user with the need of an external build.
                                compileTimeSyntaxTreesBuilder[change.FilePath] = change.NewTree;
                                Logger.DesignTime.Trace?.Log( $"Compile-time change detected: {change.FilePath} is a new compile-time syntax tree." );
                                OnCompileTimeChange( false );

                                break;

                            case CompileTimeChangeKind.NoLongerCompileTime:
                                compileTimeSyntaxTreesBuilder.Remove( change.FilePath );
                                Logger.DesignTime.Trace?.Log( $"Compile-time change detected: : {change.FilePath} no longer contains compile-time code." );
                                OnCompileTimeChange( false );

                                break;
                        }
                    }

                    newCompileTimeSyntaxTrees = compileTimeSyntaxTreesBuilder.ToImmutable();
                }
                else
                {
                    newCompileTimeSyntaxTrees = this.CompileTimeSyntaxTrees ?? _emptyCompileTimeSyntaxTrees;
                }

                // Return the new state.
                var newCompilationResult = invalidateCompilationResult ? this.PipelineResult.Invalidate( newChanges ) : this.PipelineResult;

                newState = new PipelineState(
                    newState,
                    newCompileTimeSyntaxTrees,
                    newStatus,
                    newTracker,
                    newCompilationResult,
                    newConfiguration );

                return newState;

                // Local method called when a change is detected in compile-time code. Returns a value specifying is logging is required.
                void OnCompileTimeChange( bool requiresRebuild )
                {
                    invalidateCompilationResult = false;

                    if ( newState.Status == DesignTimeAspectPipelineStatus.Ready )
                    {
                        Logger.DesignTime.Trace?.Log(
                            $"DesignTimeAspectPipeline.InvalidateCache('{newCompilation.AssemblyName}'): compile-time change detected." );

                        var pipeline = newState._pipeline;

                        if ( requiresRebuild )
                        {
                            Logger.DesignTime.Trace?.Log( "Requiring an external rebuild." );

                            newStatus = DesignTimeAspectPipelineStatus.NeedsExternalBuild;

                            if ( pipeline.ProjectOptions.BuildTouchFile != null && File.Exists( pipeline.ProjectOptions.BuildTouchFile ) )
                            {
                                using var mutex = MutexHelper.WithGlobalLock( pipeline.ProjectOptions.BuildTouchFile );
                                File.Delete( pipeline.ProjectOptions.BuildTouchFile );
                            }
                        }
                        else
                        {
                            Logger.DesignTime.Trace?.Log( "Requiring an in-process configuration refresh." );

                            newConfiguration = null;
                            newStatus = DesignTimeAspectPipelineStatus.Default;
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
                [NotNullWhen( true )] out AspectPipelineConfiguration? configuration )
            {
                if ( state.Status == DesignTimeAspectPipelineStatus.NeedsExternalBuild && ignoreStatus )
                {
                    state = new PipelineState( state._pipeline );
                }

                Logger.DesignTime.Trace?.Log(
                    $"DesignTimeAspectPipeline.TryGetConfiguration( '{compilation.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation.Compilation )})" );

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

                        Logger.DesignTime.Trace?.Log(
                            $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation.Compilation )}) failed." );

                        configuration = null;

                        return false;
                    }
                    else
                    {
                        Logger.DesignTime.Trace?.Log(
                            $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation.Compilation )}) succeeded with new configuration {DebuggingHelper.GetObjectId( configuration )}: "
                            +
                            $"the compilation contained {compilation.Compilation.SyntaxTrees.Count()} syntax trees: {string.Join( ", ", compilation.Compilation.SyntaxTrees.Select( t => Path.GetFileName( t.FilePath ) ) )}" );

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

                    Logger.DesignTime.Trace?.Log(
                        $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation.Compilation )}) returned existing configuration {DebuggingHelper.GetObjectId( state.Configuration )}." );

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
                    compilationResult = default;

                    return false;
                }

                // Execute the pipeline.
                var success = state._pipeline.TryExecute( compilation, diagnosticList, configuration, cancellationToken, out var pipelineResult );

                var additionalSyntaxTrees = pipelineResult switch
                {
                    null => ImmutableArray<IntroducedSyntaxTree>.Empty,
                    _ => pipelineResult.AdditionalSyntaxTrees
                };

                var result = new DesignTimePipelineExecutionResult(
                    success,
                    compilation.SyntaxTrees,
                    additionalSyntaxTrees,
                    new ImmutableUserDiagnosticList(
                        diagnosticList.ToImmutableArray(),
                        pipelineResult?.Diagnostics.DiagnosticSuppressions,
                        pipelineResult?.Diagnostics.CodeFixes ),
                    pipelineResult?.ExternallyInheritableAspects.Select( i => new InheritableAspectInstance( i ) ).ToImmutableArray()
                    ?? ImmutableArray<InheritableAspectInstance>.Empty,
                    pipelineResult?.ExternallyVisibleValidators ?? ImmutableArray<ReferenceValidatorInstance>.Empty );

                var directoryOptions = state._pipeline.ServiceProvider.GetRequiredService<IPathOptions>();
                UserDiagnosticRegistrationService.GetInstance( directoryOptions ).RegisterDescriptors( result );

                // We intentionally commit the pipeline state here so that the caller, not us, can decide what part of the work should be committed
                // in case of cancellation. From our point of view, this is a safe place to commit.
                state = state.SetPipelineResult( compilation, result );

                // Execute the validators. We have to run them even if we have no user validator because this also runs system validators.
                ExecuteValidators( ref state, compilation, configuration, cancellationToken );

                compilationResult = new CompilationResult( state.PipelineResult, state.ValidationResult );

                return true;
            }

            private static void ExecuteValidators(
                ref PipelineState state,
                PartialCompilation compilation,
                AspectPipelineConfiguration configuration,
                CancellationToken cancellationToken )
            {
                var validationRunner = new DesignTimeValidatorRunner(
                    state.Configuration!.ServiceProvider,
                    state.PipelineResult,
                    configuration.ProjectModel,
                    state._pipeline );

                IEnumerable<SyntaxTree> syntaxTreesToValidate;

                if ( state.PipelineResult.Validators.EqualityKey == state.ValidationResult.ValidatorEqualityKey )
                {
                    // If validators did not change, we only have to validate syntax trees that have changed.
                    // (we actually received a closure of modified syntax trees, so the flow could be optimized).
                    syntaxTreesToValidate = compilation.SyntaxTrees.Values;
                }
                else
                {
                    // If validators did change, we need to validate all syntax trees.
                    syntaxTreesToValidate = compilation.Compilation.SyntaxTrees;
                }

                var syntaxTreeDictionaryBuilder = state.ValidationResult.SyntaxTreeResults.ToBuilder();

                var userDiagnosticSink = new UserDiagnosticSink( configuration.CompileTimeProject, null );

                foreach ( var syntaxTree in syntaxTreesToValidate )
                {
                    userDiagnosticSink.Reset();
                    var semanticModel = compilation.Compilation.GetSemanticModel( syntaxTree );
                    validationRunner.Validate( semanticModel, userDiagnosticSink, cancellationToken );

                    if ( !userDiagnosticSink.IsEmpty )
                    {
                        var diagnostics = userDiagnosticSink.ToImmutable();

                        var syntaxTreeResult = new SyntaxTreeValidationResult(
                            syntaxTree,
                            diagnostics.ReportedDiagnostics,
                            diagnostics.DiagnosticSuppressions.Select( d => new CacheableScopedSuppression( d ) ).ToImmutableArray() );

                        syntaxTreeDictionaryBuilder[syntaxTree.FilePath] = syntaxTreeResult;
                    }
                    else
                    {
                        syntaxTreeDictionaryBuilder.Remove( syntaxTree.FilePath );
                    }
                }

                // TODO: remove trees that no longer exist in the compilation.

                var newValidationResult = new CompilationValidationResult(
                    syntaxTreeDictionaryBuilder.ToImmutable(),
                    state.PipelineResult.Validators.EqualityKey );

                state = state.SetValidationResult( newValidationResult );
            }

            private PipelineState SetValidationResult( CompilationValidationResult validationResult )
            {
                return new PipelineState( this, validationResult );
            }

            private PipelineState SetPipelineResult( PartialCompilation compilation, DesignTimePipelineExecutionResult pipelineResult )
            {
                var compilationResult = this.PipelineResult.Update( compilation, pipelineResult );

                return new PipelineState( this, this._compilationChangeTracker.ResetUnprocessedChanges(), compilationResult );
            }
        }
    }
}