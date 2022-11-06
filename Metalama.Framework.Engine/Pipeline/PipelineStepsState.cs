// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline;

/// <summary>
/// Maintains the state of <see cref="HighLevelPipelineStage"/>, composed of several <see cref="PipelineStep"/>.
/// The current object is essentially a mutable list of <see cref="PipelineStep"/> instances. It exposes methods
/// like <see cref="AddAspectInstances"/> or <see cref="AddAspectSources"/> that
/// allow to add inputs to different steps of the pipeline. This object must create the steps in the appropriate order.
/// </summary>
internal class PipelineStepsState : IPipelineStepsResult, IDiagnosticAdder
{
    private readonly SkipListDictionary<PipelineStepId, PipelineStep> _steps;
    private readonly PipelineStepIdComparer _comparer;
    private readonly UserDiagnosticSink _diagnostics;
    private readonly ConcurrentLinkedList<ITransformation> _transformations = new();
    private readonly ConcurrentLinkedList<IAspectInstance> _inheritableAspectInstances = new();
    private readonly ConcurrentLinkedList<AspectInstanceResult> _aspectInstanceResults = new();
    private readonly ConcurrentLinkedList<IValidatorSource> _validatorSources = new();
    private readonly OverflowAspectSource _overflowAspectSource = new();
    private readonly IntrospectionPipelineListener? _introspectionListener;
    private readonly bool _shouldDetectUnorderedAspects;

    private PipelineStep? _currentStep;

    public CompilationModel LastCompilation { get; private set; }

    public ImmutableArray<CompilationModel> Compilations { get; private set; }

    public IReadOnlyCollection<ITransformation> Transformations => this._transformations;

    public ImmutableArray<IAspectInstance> InheritableAspectInstances => this._inheritableAspectInstances.ToImmutableArray();

    public ImmutableArray<IValidatorSource> ValidatorSources => this._validatorSources.ToImmutableArray();

    ImmutableUserDiagnosticList IPipelineStepsResult.Diagnostics => this._diagnostics.ToImmutable();

    public ImmutableArray<IAspectSource> ExternalAspectSources => ImmutableArray.Create<IAspectSource>( this._overflowAspectSource );

    public ImmutableArray<AspectInstanceResult> AspectInstanceResults => this._aspectInstanceResults.ToImmutableArray();

    public AspectPipelineConfiguration PipelineConfiguration { get; }

    public PipelineStepsState(
        IReadOnlyList<OrderedAspectLayer> aspectLayers,
        CompilationModel inputLastCompilation,
        ImmutableArray<IAspectSource> inputAspectSources,
        ImmutableArray<IValidatorSource> inputValidatorSources,
        AspectPipelineConfiguration pipelineConfiguration,
        CancellationToken cancellationToken )
    {
        this._introspectionListener = pipelineConfiguration.ServiceProvider.GetService<IntrospectionPipelineListener>();
        this._shouldDetectUnorderedAspects = pipelineConfiguration.ServiceProvider.GetRequiredService<IProjectOptions>().RequireOrderedAspects;

        this._diagnostics = new UserDiagnosticSink( pipelineConfiguration.CompileTimeProject, pipelineConfiguration.CodeFixFilter );
        this.LastCompilation = inputLastCompilation;
        this.PipelineConfiguration = pipelineConfiguration;
        this.Compilations = ImmutableArray.Create( inputLastCompilation );

        // Create an empty collection of steps.
        this._comparer = new PipelineStepIdComparer( aspectLayers );
        this._steps = new SkipListDictionary<PipelineStepId, PipelineStep>( this._comparer );

        // Add the initial steps.
        foreach ( var aspectLayer in aspectLayers )
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ( aspectLayer.AspectLayerId.IsDefault )
            {
                var step = new EvaluateAspectSourcesPipelineStep( this, aspectLayer );

                _ = this._steps.Add( step.Id, step );
            }
        }

        // Add the initial sources.
        // TODO: process failure of the next line.
        this.AddAspectSources( inputAspectSources, cancellationToken );
        this.AddValidatorSources( inputValidatorSources );
    }

    public async Task ExecuteAsync( CancellationToken cancellationToken )
    {
        using var enumerator = this._steps.GetEnumerator();
        PipelineStep? previousStep = null;

        var stepIndex = 0;

        while ( enumerator.MoveNext() )
        {
            cancellationToken.ThrowIfCancellationRequested();

            this._currentStep = enumerator.Current.Value;

            this.DetectUnorderedSteps( ref previousStep, this._currentStep );

            var compilation = this.LastCompilation.GetCompilationModel();

            this.LastCompilation = await this._currentStep!.ExecuteAsync( compilation, stepIndex, cancellationToken );

            if ( !ReferenceEquals( compilation, this.LastCompilation ) )
            {
                this.Compilations = this.Compilations.Add( this.LastCompilation );
            }

            stepIndex++;
        }
    }

    private void DetectUnorderedSteps( ref PipelineStep? previousStep, PipelineStep currentStep )
    {
        if ( previousStep != null && this._shouldDetectUnorderedAspects )
        {
            if ( previousStep.AspectLayer != currentStep.AspectLayer && previousStep.AspectLayer.ExplicitOrder >= currentStep.AspectLayer.ExplicitOrder )
            {
                this._diagnostics.Report(
                    GeneralDiagnosticDescriptors.UnorderedLayers.CreateRoslynDiagnostic(
                        null,
                        (previousStep.AspectLayer.AspectLayerId.ToString(), currentStep.AspectLayer.AspectLayerId.ToString()) ) );
            }

            if ( this._comparer.Compare( currentStep.Id, previousStep.Id ) < 0 )
            {
                throw new AssertionFailedException( "Steps with lower depth must be processed before steps with higher depth." );
            }
        }

        previousStep = currentStep;
    }

    public bool AddAspectSources( IEnumerable<IAspectSource> aspectSources, CancellationToken cancellationToken )
    {
        var success = true;

        foreach ( var aspectSource in aspectSources )
        {
            foreach ( var aspectType in aspectSource.AspectClasses )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( this._currentStep?.AspectLayer.AspectClass == aspectType )
                {
                    // When an aspect class is adding the an instance of the same aspect class, we need to evaluate
                    // the aspect source immediately. This is done by the caller, and not here, so we skip it.
                }
                else
                {
                    var aspectLayerId = new AspectLayerId( aspectType );

                    if ( !this._comparer.Contains( aspectLayerId ) )
                    {
                        // This is an aspect of a different stage.
                        this._overflowAspectSource.Add( aspectSource, aspectType );
                    }
                    else
                    {
                        // There is a unique depth and TargetKind for the AspectSource step step.
                        var stepId = new PipelineStepId( aspectLayerId, -1, -1, PipelineStepPhase.Initialize, -1 );

                        if ( !this.TryGetOrAddStep( stepId, false, out var step ) )
                        {
                            this._diagnostics.Report(
                                GeneralDiagnosticDescriptors.CannotAddChildAspectToPreviousPipelineStep.CreateRoslynDiagnostic(
                                    this._currentStep!.AspectLayer.AspectClass.DiagnosticLocation,
                                    (this._currentStep.AspectLayer.AspectClass.ShortName, aspectType.ShortName) ) );

                            success = false;

                            continue;
                        }

                        var typedStep = (EvaluateAspectSourcesPipelineStep) step;
                        typedStep.AddAspectSource( aspectSource );
                    }
                }
            }
        }

        return success;
    }

    public ImmutableArray<AspectInstance> ExecuteAspectSource(
        CompilationModel compilation,
        IAspectClass aspectClass,
        IEnumerable<IAspectSource> aspectSources,
        CancellationToken cancellationToken )
    {
        var aspectSourceResults = aspectSources.Where( a => a.AspectClasses.Contains( aspectClass ) )
            .Select( s => s.GetAspectInstances( compilation, aspectClass, this, cancellationToken ) )
            .ToList();

        HashSet<IDeclaration>? exclusions = null;

        foreach ( var exclusion in aspectSourceResults.SelectMany( x => x.Exclusions ) )
        {
            exclusions ??= new HashSet<IDeclaration>( ReferenceEqualityComparer<IDeclaration>.Instance );

            exclusions.Add( exclusion.GetTarget( compilation ) );
        }

        bool IsExcluded( IDeclaration declaration )
        {
            if ( exclusions == null || exclusions.Count == 0 )
            {
                return false;
            }

            if ( exclusions.Contains( declaration ) )
            {
                return true;
            }

            if ( declaration.ContainingDeclaration != null )
            {
                return IsExcluded( declaration.ContainingDeclaration );
            }

            return false;
        }

        var aspectInstances = aspectSourceResults
            .SelectMany( x => x.AspectInstances )
            .Select(
                x =>
                {
                    var target = (IDeclarationImpl) x.TargetDeclaration.GetTarget( compilation );

                    if ( !x.Predecessors.IsDefaultOrEmpty && x.Predecessors[0].Kind != AspectPredecessorKind.Attribute && IsExcluded( target ) )
                    {
                        return default;
                    }

                    return new ResolvedAspectInstance( x, target, x.ComputeEligibility( target ) );
                } )
            .Where( x => x.TargetDeclaration != null! )
            .ToList();

        // Process aspect requirements.
        // We always add an aspect instance if there is a requirement, even if there is already an instance, because this has the side effect of exposing
        // the predecessors to the IAspectInstance in the last instance of the aggregated aspect.
        var requirements = aspectSourceResults.SelectMany( x => x.Requirements )
            .Select( x => (TargetDeclaration: x.TargetDeclaration.GetTarget( compilation ), x.Predecessor) )
            .GroupBy( x => x.TargetDeclaration );

        foreach ( var requirement in requirements )
        {
            var requirementTarget = requirement.Key;
            var predecessors = requirement.Select( x => new AspectPredecessor( AspectPredecessorKind.RequiredAspect, x.Predecessor ) );

            var stronglyTypedAspectClass = (AspectClass) aspectClass;
            var aspect = stronglyTypedAspectClass.CreateDefaultInstance();

            var aspectInstance = new AspectInstance( aspect, requirementTarget, stronglyTypedAspectClass, predecessors.ToImmutableArray() );
            var eligibility = aspectInstance.ComputeEligibility( requirementTarget );

            if ( (eligibility & (EligibleScenarios.Aspect | EligibleScenarios.Inheritance)) != 0 )
            {
                aspectInstances.Add( new ResolvedAspectInstance( aspectInstance, (IDeclarationImpl) requirementTarget, eligibility ) );
            }
            else
            {
                // The situation should have been detected before.
                throw new AssertionFailedException( $"Cannot add the aspect '{aspectClass.ShortName}' to '{requirementTarget}' because of eligibility. " );
            }
        }

        // We assume that all aspect instances are eligible, but some are eligible only for inheritance.

        // Get the aspects that can be processed, i.e. they are not abstract-only.
        var concreteAspectInstances = aspectInstances
            .Where( a => a.Eligibility.IncludesAll( EligibleScenarios.Aspect ) )
            .ToList();

        // Gets aspects that can be inherited.
        var inheritableAspectInstances = aspectInstances
            .Where( a => a.Eligibility.IncludesAll( EligibleScenarios.Inheritance ) && a.AspectInstance.AspectClass.IsInherited )
            .ToList();

        // Gets aspects that have been inherited by the source. 
        var inheritedAspectInstancesInProject = inheritableAspectInstances
            .SelectMany(
                a => a.TargetDeclaration.GetDerivedDeclarations()
                    .Where( d => !IsExcluded( d ) )
                    .Select( d => (TargetDeclaration: (IDeclarationImpl) d, DerivedAspectInstance: a.AspectInstance.CreateDerivedInstance( d )) )
                    .Where( x => x.DerivedAspectInstance.ComputeEligibility( x.TargetDeclaration ).IncludesAll( EligibleScenarios.Aspect ) )
                    .Select(
                        x =>
                            new ResolvedAspectInstance(
                                x.DerivedAspectInstance,
                                x.TargetDeclaration,
                                EligibleScenarios.Aspect ) ) )
            .ToList();

        // Index these aspects. 
        this.AddAspectInstances( concreteAspectInstances );
        this.AddAspectInstances( inheritedAspectInstancesInProject );
        this.AddInheritableAspectInstances( inheritableAspectInstances.Select( x => x.AspectInstance ).ToList() );

        return concreteAspectInstances.Select( x => x.AspectInstance ).ToImmutableArray();
    }

    private bool TryGetOrAddStep(
        in PipelineStepId stepId,
        bool allowAddToCurrentLayer,
        [NotNullWhen( true )] out PipelineStep? step )
    {
        var aspectLayer = this._comparer.GetOrderedAspectLayer( stepId.AspectLayerId );

        if ( this._currentStep != null )
        {
            var currentLayerOrder = this._currentStep.AspectLayer.Order;

            if ( aspectLayer.Order < currentLayerOrder ||
                 (stepId.AspectLayerId == this._currentStep.AspectLayer.AspectLayerId
                  && (!allowAddToCurrentLayer || this._comparer.Compare( stepId, this._currentStep.Id ) < 0)) )
            {
                // Cannot add a step before the current one.
                step = null;

                return false;
            }
        }

        if ( !this._steps.TryGetValue( stepId, out step ) )
        {
            lock ( this._steps )
            {
                if ( !this._steps.TryGetValue( stepId, out step ) )
                {
                    step = new ExecuteAspectLayerPipelineStep( this, stepId, aspectLayer );
                    _ = this._steps.Add( stepId, step );
                }
            }
        }

        return true;
    }

    public void AddAspectInstances( IEnumerable<ResolvedAspectInstance> aspectInstances )
    {
        foreach ( var aspectInstance in aspectInstances )
        {
            foreach ( var layer in aspectInstance.AspectInstance.AspectClass.Layers )
            {
                var aspectTargetDeclaration = (IDeclaration) aspectInstance.TargetDeclaration;
                var aspectTargetTypeDeclaration = aspectTargetDeclaration.GetClosestNamedType() ?? aspectTargetDeclaration;

                var stepId = new PipelineStepId(
                    new AspectLayerId( aspectInstance.AspectInstance.AspectClass, layer.LayerName ),
                    this.LastCompilation.GetDepth( aspectTargetTypeDeclaration ),
                    this.LastCompilation.GetDepth( aspectTargetDeclaration ),
                    PipelineStepPhase.Initialize,
                    -1 );

                if ( this._currentStep != null && this._comparer.Compare( this._currentStep.Id, stepId ) >= 0 )
                {
                    // The aspect is trying to add an aspect of the same type, which is legal, but to a declaration
                    // of lower depth.
                    var parentAspect = (AspectInstance) aspectInstance.AspectInstance.Predecessors[0].Instance;
                    var parentTarget = parentAspect.TargetDeclaration.GetTarget( aspectTargetDeclaration.Compilation );

                    this.Report(
                        GeneralDiagnosticDescriptors.CannotAddAspectToPreviousPipelineStep.CreateRoslynDiagnostic(
                            parentAspect.GetDiagnosticLocation( aspectTargetDeclaration.Compilation.GetRoslynCompilation() ),
                            (parentAspect.AspectClass.ShortName, parentTarget, parentTarget.DeclarationKind, aspectTargetDeclaration,
                             aspectTargetDeclaration.DeclarationKind) ) );
                }
                else if ( !this.TryGetOrAddStep( stepId, true, out var step ) )
                {
                    // This should not happen here. The source should not have been added.
                    throw new AssertionFailedException( $"A pipeline step was added for '{stepId}'." );
                }
                else
                {
                    ((ExecuteAspectLayerPipelineStep) step).AddAspectInstance( aspectInstance );
                }
            }
        }
    }

    public void AddInheritableAspectInstances( IReadOnlyList<AspectInstance> inheritedAspectInstances )
    {
        foreach ( var aspectInstance in inheritedAspectInstances )
        {
            this._inheritableAspectInstances.Add( aspectInstance );
        }
    }

    public void AddDiagnostics( ImmutableUserDiagnosticList diagnostics )
    {
        this._diagnostics.Report( diagnostics.ReportedDiagnostics );
        this._diagnostics.Suppress( diagnostics.DiagnosticSuppressions );
        this._diagnostics.AddCodeFixes( diagnostics.CodeFixes );
    }

    public void AddTransformations( IEnumerable<ITransformation> transformations )
    {
        foreach ( var transformation in transformations )
        {
            this._transformations.Add( transformation );
        }
    }

    public bool AddValidatorSources( IEnumerable<IValidatorSource> validatorSources )
    {
        foreach ( var source in validatorSources )
        {
            this._validatorSources.Add( source );
        }

        return true;
    }

    public void Report( Diagnostic diagnostic ) => this._diagnostics.Report( diagnostic );

    public void AddAspectInstanceResult( AspectInstanceResult aspectInstanceResult )
    {
        this._aspectInstanceResults.Add( aspectInstanceResult );
        this._introspectionListener?.AddAspectResult( aspectInstanceResult );
    }
}