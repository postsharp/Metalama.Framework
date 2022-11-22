// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption;
using Metalama.Framework.Aspects;
using Metalama.Framework.DesignTime.Diagnostics;
using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.DesignTime.Pipeline.Diff;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Engine.Validation;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline
{
    internal partial class DesignTimeAspectPipeline
    {
        internal readonly struct PipelineState
        {
            private readonly DesignTimeAspectPipeline _pipeline;
            private readonly DependencyGraph _dependencies;

            private static readonly ImmutableDictionary<string, SyntaxTree?> _emptyCompileTimeSyntaxTrees =
                ImmutableDictionary.Create<string, SyntaxTree?>( StringComparer.Ordinal );

            // Syntax trees that may have compile time code based on namespaces. When a syntax tree is known to be compile-time but
            // has been invalidated, we don't remove it from the dictionary, but we set its value to null. It allows to know
            // that this specific tree is outdated, which then allows us to display a warning.

            public ImmutableDictionary<string, SyntaxTree?>? CompileTimeSyntaxTrees { get; }

            public FallibleResultWithDiagnostics<AspectPipelineConfiguration>? Configuration { get; }

            internal DesignTimeAspectPipelineStatus Status { get; }

            public ProjectVersion? ProjectVersion { get; }

            public CompilationPipelineResult PipelineResult { get; }

            public CompilationValidationResult ValidationResult { get; }

            internal PipelineState( DesignTimeAspectPipeline pipeline )
            {
                this._pipeline = pipeline;
                this._dependencies = DependencyGraph.Empty;
                this.PipelineResult = new CompilationPipelineResult();
                this.ValidationResult = CompilationValidationResult.Empty;
                this.CompileTimeSyntaxTrees = null;
                this.Configuration = null;
                this.Status = DesignTimeAspectPipelineStatus.Default;
                this.ProjectVersion = null;
            }

            private PipelineState( PipelineState prototype )
            {
                this._pipeline = prototype._pipeline;
                this.ProjectVersion = prototype.ProjectVersion;
                this.CompileTimeSyntaxTrees = prototype.CompileTimeSyntaxTrees;
                this.Configuration = prototype.Configuration;
                this.Status = prototype.Status;
                this.PipelineResult = prototype.PipelineResult;
                this.ValidationResult = prototype.ValidationResult;
                this._dependencies = prototype._dependencies;
            }

            private PipelineState(
                PipelineState prototype,
                FallibleResultWithDiagnostics<AspectPipelineConfiguration> configuration,
                DesignTimeAspectPipelineStatus status ) : this( prototype )
            {
                this.Configuration = configuration;
                this.Status = status;
            }

            private PipelineState(
                PipelineState prototype,
                ImmutableDictionary<string, SyntaxTree?> compileTimeSyntaxTrees,
                DesignTimeAspectPipelineStatus status,
                ProjectVersion projectVersion,
                CompilationPipelineResult pipelineResult,
                DependencyGraph dependencies,
                FallibleResultWithDiagnostics<AspectPipelineConfiguration>? configuration )
                : this( prototype )
            {
                this.CompileTimeSyntaxTrees = compileTimeSyntaxTrees;
                this.Status = status;
                this.ProjectVersion = projectVersion;
                this.PipelineResult = pipelineResult;
                this.Configuration = configuration;
                this._dependencies = dependencies;
            }

            private PipelineState( PipelineState prototype, ImmutableDictionary<string, SyntaxTree?> compileTimeSyntaxTrees )
                : this( prototype )
            {
                this.CompileTimeSyntaxTrees = compileTimeSyntaxTrees;
            }

            private PipelineState( PipelineState prototype, DesignTimeAspectPipelineStatus status )
                : this( prototype )
            {
                this.Status = status;
            }

            private PipelineState(
                PipelineState prototype,
                ProjectVersion projectVersion,
                CompilationPipelineResult pipelineResult,
                DependencyGraph dependencies ) : this( prototype )
            {
                this.PipelineResult = pipelineResult;
                this.ProjectVersion = projectVersion;
                this._dependencies = dependencies;
            }

            private PipelineState( PipelineState prototype, CompilationValidationResult validationResult ) : this( prototype )
            {
                this.ValidationResult = validationResult;
            }

            private static IReadOnlyList<SyntaxTree> GetCompileTimeSyntaxTrees(
                ref PipelineState state,
                Compilation compilation,
                TestableCancellationToken cancellationToken )
            {
                List<SyntaxTree> trees = new( state.CompileTimeSyntaxTrees?.Count ?? 8 );

                if ( state.CompileTimeSyntaxTrees == null )
                {
                    // The cache has not been set yet, so we need to compute the value from zero.

                    if ( state.ProjectVersion?.Compilation != null && state.ProjectVersion.Compilation != compilation )
                    {
                        throw new AssertionFailedException( $"Compilation mismatch with '{compilation.Assembly.Identity}'." );
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

            public PipelineState Reset() => new( this._pipeline );

            /// <summary>
            /// Invalidates the cache given a new <see cref="Compilation"/>.
            /// </summary>
            internal async ValueTask<PipelineState> InvalidateCacheForNewCompilationAsync(
                Compilation newCompilation,
                bool invalidateCompilationResult,
                TestableCancellationToken cancellationToken )
            {
                var newStatus = this.Status;
                var newState = this;
                var newConfiguration = this.Configuration;

                // Detect changes in the syntax trees of the tracked compilation.
                var newChanges = await this._pipeline.ProjectVersionProvider.GetCompilationChangesAsync(
                    this.ProjectVersion?.Compilation,
                    newCompilation,
                    cancellationToken );

                ImmutableDictionary<string, SyntaxTree?> newCompileTimeSyntaxTrees;
                DependencyGraph newDependencyGraph;
                CompilationPipelineResult newCompilationResult;

                if ( newChanges.HasCompileTimeCodeChange )
                {
                    if ( this.CompileTimeSyntaxTrees == null && newChanges.IsIncremental )
                    {
                        throw new AssertionFailedException( "Got an incremental compilation change, but _compileTimeSyntaxTrees is null." );
                    }

                    // If there is a compile-time change in references, signal a compile-time change for the current project but do not pause the pipeline.
                    if ( !newChanges.ReferencedPortableExecutableChanges.IsEmpty ||
                         newChanges.ReferencedCompilationChanges.Any( c => c.Value.HasCompileTimeCodeChange ) )
                    {
                        OnCompileTimeChange( this._pipeline.Logger, false );
                    }

                    var compileTimeSyntaxTreesBuilder = this.CompileTimeSyntaxTrees?.ToBuilder()
                                                        ?? ImmutableDictionary.CreateBuilder<string, SyntaxTree?>( StringComparer.Ordinal );

                    foreach ( var changeEntry in newChanges.SyntaxTreeChanges )
                    {
                        var change = changeEntry.Value;

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
                                compileTimeSyntaxTreesBuilder[change.FilePath] = change.NewTree;
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

                    if ( invalidateCompilationResult )
                    {
                        newDependencyGraph = DependencyGraph.Empty;
                        newCompilationResult = new CompilationPipelineResult();
                    }
                    else
                    {
                        // The pipeline is paused. We do not invalidate the results to we can still serve the old ones.

                        newDependencyGraph = this._dependencies;
                        newCompilationResult = this.PipelineResult;
                    }
                }
                else
                {
                    // Compile-time trees are unchanged.
                    newCompileTimeSyntaxTrees = this.CompileTimeSyntaxTrees ?? _emptyCompileTimeSyntaxTrees;

                    if ( newChanges.HasChange )
                    {
                        var invalidator = this.PipelineResult.ToInvalidator();

                        newDependencyGraph = await this._pipeline.ProjectVersionProvider.ProcessCompilationChangesAsync(
                            newChanges,
                            this._dependencies,
                            invalidator.InvalidateSyntaxTree,
                            false,
                            cancellationToken );

                        newCompilationResult = invalidator.ToImmutable();
                    }
                    else
                    {
                        newDependencyGraph = this._dependencies;
                        newCompilationResult = this.PipelineResult;
                    }
                }

                // Return the new state.

                newState = new PipelineState(
                    newState,
                    newCompileTimeSyntaxTrees,
                    newStatus,
                    newChanges.NewProjectVersion,
                    newCompilationResult,
                    newDependencyGraph,
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
                    else if ( newState.Status == DesignTimeAspectPipelineStatus.Default )
                    {
                        // We may have cached an invalid configuration, so if we have a compile-time change, reset this.
                        newConfiguration = null;
                    }
                }
            }

            internal static FallibleResultWithDiagnostics<AspectPipelineConfiguration> GetConfiguration(
                ref PipelineState state,
                PartialCompilation compilation,
                bool ignoreStatus,
                TestableCancellationToken cancellationToken )
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

                    state._pipeline._observer?.OnInitializePipeline( compilation.Compilation );

                    var diagnosticAdder = new DiagnosticBag();

                    var licenseConsumptionManager = state._pipeline.ServiceProvider.GetBackstageService<ILicenseConsumptionManager>();
                    var redistributionLicenseKey = licenseConsumptionManager?.RedistributionLicenseKey;

                    var projectLicenseInfo = string.IsNullOrEmpty( redistributionLicenseKey )
                        ? ProjectLicenseInfo.Empty
                        : new ProjectLicenseInfo( redistributionLicenseKey );

                    if ( !state._pipeline.TryInitialize(
                            diagnosticAdder,
                            compilation,
                            projectLicenseInfo,
                            compileTimeTrees,
                            cancellationToken,
                            out var configuration ) )
                    {
                        // A failure here means an error or a cache miss.

                        state._pipeline.Logger.Warning?.Log(
                            $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation.Compilation )}) failed: cannot initialize." );

                        return FallibleResultWithDiagnostics<AspectPipelineConfiguration>.Failed( diagnosticAdder.ToImmutableArray() );
                    }
                    else
                    {
                        state._pipeline.Logger.Trace?.Log(
                            $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation.Compilation )}) succeeded with new configuration {DebuggingHelper.GetObjectId( configuration )}: "
                            +
                            $"the compilation contained {compilation.Compilation.SyntaxTrees.Count()} syntax trees: {string.Join( ", ", compilation.Compilation.SyntaxTrees.Select( t => Path.GetFileName( t.FilePath ) ) )}" );

                        var result = FallibleResultWithDiagnostics<AspectPipelineConfiguration>.Succeeded( configuration, diagnosticAdder.ToImmutableArray() );
                        state = new PipelineState( state, result, DesignTimeAspectPipelineStatus.Ready );

                        return result;
                    }
                }
                else if ( !state.Configuration.Value.IsSuccessful )
                {
                    // We have a cached configuration, but a failed one.

                    state._pipeline.Logger.Warning?.Log(
                        $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation.Compilation )}) failed: a previous initialization of the pipeline has failed and there is no change." );

                    return FallibleResultWithDiagnostics<AspectPipelineConfiguration>.Failed( state.Configuration.Value.Diagnostics );
                }
                else
                {
                    if ( state.Status == DesignTimeAspectPipelineStatus.Paused )
                    {
                        // We have an outdated configuration because the pipeline is paused.

                        state._pipeline.Logger.Warning?.Log(
                            $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation.Compilation )}) failed: the pipeline is paused." );

                        return FallibleResultWithDiagnostics<AspectPipelineConfiguration>.Failed( ImmutableArray<Diagnostic>.Empty );
                    }
                    else
                    {
                        // We have a valid configuration and it is not outdated.

                        state._pipeline.Logger.Trace?.Log(
                            $"DesignTimeAspectPipeline.TryGetConfiguration('{compilation.Compilation.AssemblyName}', CompilationId = {DebuggingHelper.GetObjectId( compilation.Compilation )}) returned existing configuration {DebuggingHelper.GetObjectId( state.Configuration.Value.Value )}." );

                        return state.Configuration.Value;
                    }
                }
            }

            /// <summary>
            /// Executes the pipeline.
            /// </summary>
            public static async Task<( FallibleResultWithDiagnostics<CompilationResult> CompilationResult, PipelineState NewState)> ExecuteAsync(
                PipelineState state,
                PartialCompilation compilation,
                DesignTimeProjectVersion projectVersion,
                TestableCancellationToken cancellationToken )
            {
                DiagnosticBag diagnosticBag = new();

                if ( state.Status == DesignTimeAspectPipelineStatus.Paused )
                {
                    throw new InvalidOperationException();
                }

                var getConfigurationResult = GetConfiguration( ref state, compilation, false, cancellationToken );

                if ( !getConfigurationResult.IsSuccessful )
                {
                    if ( state._pipeline.Logger.Error != null )
                    {
                        var errors = getConfigurationResult.Diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).ToList();

                        state._pipeline.Logger.Error?.Log( $"TryGetConfiguration('{compilation.Compilation.AssemblyName}') failed: {errors.Count} reported." );

                        foreach ( var diagnostic in errors )
                        {
                            state._pipeline.Logger.Error?.Log( diagnostic.ToString() );
                        }
                    }

                    state = new PipelineState( state, getConfigurationResult, DesignTimeAspectPipelineStatus.Default );

                    return (FallibleResultWithDiagnostics<CompilationResult>.Failed( getConfigurationResult.Diagnostics ), state);
                }

                var configuration = getConfigurationResult.Value;

                // Execute the pipeline.
                var dependencyCollector = new DependencyCollector(
                    configuration.ServiceProvider.WithService( projectVersion ),
                    projectVersion.ProjectVersion,
                    compilation );

                compilation.DerivedTypes.PopulateDependencies( dependencyCollector );
                var serviceProvider = configuration.ServiceProvider.WithServices( dependencyCollector, projectVersion );

                var pipelineResult = await state._pipeline.ExecuteAsync(
                    compilation,
                    diagnosticBag,
                    configuration.WithServiceProvider( serviceProvider ),
                    cancellationToken );

                var pipelineResultValue = pipelineResult.IsSuccessful ? pipelineResult.Value : null;

#if DEBUG
                dependencyCollector.Freeze();
#endif

                var additionalSyntaxTrees = pipelineResultValue switch
                {
                    null => ImmutableArray<IntroducedSyntaxTree>.Empty,
                    _ => pipelineResultValue.AdditionalSyntaxTrees
                };

                var inheritableAspectInstances =
                    pipelineResultValue?.ExternallyInheritableAspects.SelectImmutableArray( i => new InheritableAspectInstance( i ) )
                    ?? ImmutableArray<InheritableAspectInstance>.Empty;

                var externallyVisibleValidators = pipelineResultValue?.ExternallyVisibleValidators ?? ImmutableArray<ReferenceValidatorInstance>.Empty;

                var immutableUserDiagnostics = new ImmutableUserDiagnosticList(
                    diagnosticBag.ToImmutableArray(),
                    pipelineResultValue?.Diagnostics.DiagnosticSuppressions,
                    pipelineResultValue?.Diagnostics.CodeFixes );

                var aspectInstances = pipelineResultValue?.AspectInstances.ToImmutableArray() ?? ImmutableArray<IAspectInstance>.Empty;

                var transformations = pipelineResultValue?.Transformations ?? ImmutableArray<ITransformationBase>.Empty;

                var result = new DesignTimePipelineExecutionResult(
                    pipelineResult.IsSuccessful,
                    compilation.SyntaxTrees,
                    additionalSyntaxTrees,
                    immutableUserDiagnostics,
                    inheritableAspectInstances,
                    externallyVisibleValidators,
                    aspectInstances,
                    transformations );

                UserDiagnosticRegistrationService.GetInstance( configuration.ServiceProvider ).RegisterDescriptors( result );

                // Update the dependency graph with results of the pipeline.
                DependencyGraph newDependencies;

                if ( pipelineResult.IsSuccessful )
                {
                    if ( state._dependencies.IsUninitialized )
                    {
                        newDependencies = DependencyGraph.Create( dependencyCollector );
                    }
                    else
                    {
                        newDependencies = state._dependencies.Update( dependencyCollector );
                    }
                }
                else
                {
                    newDependencies = state._dependencies;
                }

                // We intentionally commit the pipeline state here so that the caller, not us, can decide what part of the work should be committed
                // in case of cancellation. From our point of view, this is a safe place to commit.
                state = state.SetPipelineResult( compilation, result, newDependencies );

                // Execute the validators. We have to run them even if we have no user validator because this also runs system validators.
                ExecuteValidators( ref state, compilation, configuration, cancellationToken );

                return (new CompilationResult(
                            state.ProjectVersion.AssertNotNull(),
                            state.PipelineResult,
                            state.ValidationResult,
                            configuration.CompileTimeProject,
                            state.Status ), state);
            }

            private static void ExecuteValidators(
                ref PipelineState state,
                PartialCompilation compilation,
                AspectPipelineConfiguration configuration,
                CancellationToken cancellationToken )
            {
                var semanticModelProvider = compilation.Compilation.GetSemanticModelProvider();

                var validationRunner = new DesignTimeValidatorRunner(
                    configuration.ServiceProvider,
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

                // TODO: this can be parallelized.

                foreach ( var syntaxTree in syntaxTreesToValidate )
                {
                    userDiagnosticSink.Reset();
                    var semanticModel = semanticModelProvider.GetSemanticModel( syntaxTree );
                    validationRunner.Validate( semanticModel, userDiagnosticSink, cancellationToken );

                    if ( !userDiagnosticSink.IsEmpty )
                    {
                        var diagnostics = userDiagnosticSink.ToImmutable();

                        var syntaxTreeResult = new SyntaxTreeValidationResult(
                            syntaxTree,
                            diagnostics.ReportedDiagnostics,
                            diagnostics.DiagnosticSuppressions.Select( d => new CacheableScopedSuppression( d ) )
                                .ToImmutableArray() );

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

                return new PipelineState( this, this.ProjectVersion.AssertNotNull(), compilationResult, dependencies );
            }

            public PipelineState Pause() => new( this, DesignTimeAspectPipelineStatus.Paused );
        }
    }
}