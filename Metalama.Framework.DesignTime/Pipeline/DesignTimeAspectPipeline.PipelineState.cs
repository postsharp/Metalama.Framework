// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Threading;
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

            public CompilationVersion? CompilationVersion
                => this._compilationChangeTracker.LastCompilation != null && this.Configuration != null
                    ? new CompilationVersion(
                        this._compilationChangeTracker.LastCompilation,
                        this.Configuration.CompileTimeProject?.Hash ?? 0,
                        this._compilationChangeTracker.LastTrees! )
                    : null;

            public CompilationPipelineResult PipelineResult { get; }

            public CompilationValidationResult ValidationResult { get; }

            public DependencyGraph Dependencies { get; }

            internal PipelineState( DesignTimeAspectPipeline pipeline ) : this()
            {
                this._pipeline = pipeline;
                this.PipelineResult = new CompilationPipelineResult();
                this.ValidationResult = CompilationValidationResult.Empty;
                this.Dependencies = DependencyGraph.Empty;
                this._compilationChangeTracker = new CompilationChangeTracker( new CompilationChangeTrackerStrategy( pipeline.ServiceProvider, true, true) );
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
                this.Dependencies = prototype.Dependencies;
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
                CompilationChangeTracker compilationChangeTracker,
                CompilationPipelineResult pipelineResult,
                AspectPipelineConfiguration? configuration )
                : this( prototype )
            {
                this.CompileTimeSyntaxTrees = compileTimeSyntaxTrees;
                this.Status = status;
                this._compilationChangeTracker = compilationChangeTracker;
                this.PipelineResult = pipelineResult;
                this.Configuration = configuration;
            }

            private PipelineState( PipelineState prototype, ImmutableDictionary<string, SyntaxTree?> compileTimeSyntaxTrees )
                : this( prototype )
            {
                this.CompileTimeSyntaxTrees = compileTimeSyntaxTrees;
            }

            private PipelineState(
                PipelineState prototype,
                CompilationChangeTracker compilationChangeTracker,
                CompilationPipelineResult pipelineResult,
                DependencyGraph dependencies ) : this( prototype )
            {
                this.PipelineResult = pipelineResult;
                this._compilationChangeTracker = compilationChangeTracker;
                this.Dependencies = dependencies;
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

                        if ( CompileTimeCodeFastDetector.HasCompileTimeCode( syntaxTree.GetRoot() ) )
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
                DesignTimeCompilationReferenceCollection references,
                bool invalidateCompilationResult,
                CancellationToken cancellationToken )
            {
                var newStatus = this.Status;
                var newState = this;
                var newConfiguration = this.Configuration;

                // Detect changes from dependencies.
                var dependenciesChanges = DependencyChanges.Create( this.Dependencies, references, cancellationToken );

                // Detect changes in compile-time syntax trees.
                var newTracker = this._compilationChangeTracker.Update( newCompilation, dependenciesChanges, cancellationToken );
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
                                    this._pipeline.Logger.Trace?.Log(
                                        $"Compile-time change detected: {change.FilePath} has changed. Old hash: {change.OldHash}, new hash: {change.NewHash}." );

                                    // Generated files may change during the startup sequence, and it is safe to reset the pipeline in this case.
                                    var requiresRebuild = !change.FilePath.EndsWith( ".g.cs", StringComparison.OrdinalIgnoreCase );

                                    OnCompileTimeChange( this._pipeline.Logger, requiresRebuild );
                                }

                                break;

                            case CompileTimeChangeKind.NewlyCompileTime:
                                // We don't require an external rebuild when a new syntax tree is added because Roslyn does not give us a complete
                                // compilation in the first call in the Visual Studio initializations sequence. Roslyn calls us later with
                                // a complete compilation, but we don't want to bother the user with the need of an external build.
                                compileTimeSyntaxTreesBuilder[change.FilePath] = change.NewTree.AssertNotNull();
                                this._pipeline.Logger.Trace?.Log( $"Compile-time change detected: {change.FilePath} is a new compile-time syntax tree." );
                                OnCompileTimeChange( this._pipeline.Logger, false );

                                break;

                            case CompileTimeChangeKind.NoLongerCompileTime:
                                compileTimeSyntaxTreesBuilder.Remove( change.FilePath );
                                this._pipeline.Logger.Trace?.Log( $"Compile-time change detected: : {change.FilePath} no longer contains compile-time code." );
                                OnCompileTimeChange( this._pipeline.Logger, false );

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
                void OnCompileTimeChange( ILogger logger, bool requiresRebuild )
                {
                    invalidateCompilationResult = false;

                    if ( newState.Status == DesignTimeAspectPipelineStatus.Ready )
                    {
                        logger.Trace?.Log( $"DesignTimeAspectPipeline.InvalidateCache('{newCompilation.AssemblyName}'): compile-time change detected." );

                        var pipeline = newState._pipeline;

                        if ( requiresRebuild )
                        {
                            logger.Trace?.Log( "Pausing the pipeline." );

                            newStatus = DesignTimeAspectPipelineStatus.Paused;

                            if ( pipeline.ProjectOptions.BuildTouchFile != null && File.Exists( pipeline.ProjectOptions.BuildTouchFile ) )
                            {
                                using ( MutexHelper.WithGlobalLock( pipeline.ProjectOptions.BuildTouchFile ) )
                                {
                                    if ( File.Exists( pipeline.ProjectOptions.BuildTouchFile ) )
                                    {
                                        File.Delete( pipeline.ProjectOptions.BuildTouchFile );
                                    }
                                }
                            }
                        }
                        else
                        {
                            logger.Trace?.Log( "Requiring an in-process configuration refresh." );

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
                if ( state.Status == DesignTimeAspectPipelineStatus.Paused && ignoreStatus )
                {
                    state = new PipelineState( state._pipeline );
                }

                state._pipeline.Logger.Trace?.Log(
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

                        state._pipeline.Logger.Warning?.Log(
                            $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation.Compilation )}) failed: cannot initialize." );

                        configuration = null;

                        return false;
                    }
                    else
                    {
                        state._pipeline.Logger.Trace?.Log(
                            $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation.Compilation )}) succeeded with new configuration {DebuggingHelper.GetObjectId( configuration )}: "
                            +
                            $"the compilation contained {compilation.Compilation.SyntaxTrees.Count()} syntax trees: {string.Join( ", ", compilation.Compilation.SyntaxTrees.Select( t => Path.GetFileName( t.FilePath ) ) )}" );

                        state = new PipelineState( state, configuration, DesignTimeAspectPipelineStatus.Ready );

                        return true;
                    }
                }
                else
                {
                    if ( state.Status == DesignTimeAspectPipelineStatus.Paused )
                    {
                        state._pipeline.Logger.Warning?.Log(
                            $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation.Compilation )}) failed: the pipeline is paused." );

                        configuration = null;

                        return false;
                    }

                    // We have a valid configuration and it is not outdated.

                    state._pipeline.Logger.Trace?.Log(
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
                DesignTimeCompilationReferenceCollection references,
                CancellationToken cancellationToken,
                [NotNullWhen( true )] out CompilationResult? compilationResult )
            {
                DiagnosticList diagnosticList = new();

                if ( state.Status == DesignTimeAspectPipelineStatus.Paused )
                {
                    throw new InvalidOperationException();
                }

                if ( !TryGetConfiguration( ref state, compilation, diagnosticList, false, cancellationToken, out var configuration ) )
                {
                    if ( state._pipeline.Logger.Error != null )
                    {
                        var errors = diagnosticList.Where( d => d.Severity == DiagnosticSeverity.Error ).ToList();

                        state._pipeline.Logger.Error?.Log( $"TryGetConfiguration('{compilation.Compilation.AssemblyName}') failed: {errors.Count} reported." );

                        foreach ( var diagnostic in errors )
                        {
                            state._pipeline.Logger.Error?.Log( diagnostic.ToString() );
                        }
                    }

                    compilationResult = default;

                    return false;
                }

                // Execute the pipeline.
                var dependencyCollector = new DependencyCollector(
                    configuration.ServiceProvider.WithService( references ),
                    compilation.Compilation,
                    references.References.Values.Select( x => x.CompilationVersion ) );

                compilation.DerivedTypes.PopulateDependencies( dependencyCollector );
                var serviceProvider = configuration.ServiceProvider.WithServices( dependencyCollector, references );

                var success = state._pipeline.TryExecute(
                    compilation,
                    diagnosticList,
                    configuration.WithServiceProvider( serviceProvider ),
                    cancellationToken,
                    out var pipelineResult );

#if DEBUG
                dependencyCollector.Freeze();
#endif

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

                // Update the dependency graph with results of the pipeline.
                DependencyGraph newDependencies;

                if ( success )
                {
                    newDependencies = state.Dependencies.Update( compilation.SyntaxTrees.Keys, dependencyCollector );
                }
                else
                {
                    newDependencies = state.Dependencies;
                }

                // We intentionally commit the pipeline state here so that the caller, not us, can decide what part of the work should be committed
                // in case of cancellation. From our point of view, this is a safe place to commit.
                state = state.SetPipelineResult( compilation, result, newDependencies );

                // Execute the validators. We have to run them even if we have no user validator because this also runs system validators.
                ExecuteValidators( ref state, compilation, configuration, cancellationToken );

                compilationResult = new CompilationResult(
                    state.CompilationVersion.AssertNotNull(),
                    state.PipelineResult,
                    state.ValidationResult,
                    configuration.CompileTimeProject );

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
                    configuration.ProjectModel );

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

                var userDiagnosticSink = new UserDiagnosticSink( configuration.CompileTimeProject );

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

            private PipelineState SetPipelineResult(
                PartialCompilation compilation,
                DesignTimePipelineExecutionResult pipelineResult,
                DependencyGraph dependencies )
            {
                var compilationResult = this.PipelineResult.Update( compilation, pipelineResult );

                return new PipelineState( this, this._compilationChangeTracker.ResetUnprocessedChanges(), compilationResult, dependencies );
            }
        }
    }
}