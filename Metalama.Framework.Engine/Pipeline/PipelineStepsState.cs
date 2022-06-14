// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// Maintains the state of <see cref="HighLevelPipelineStage"/>, composed of several <see cref="PipelineStep"/>.
    /// The current object is essentially a mutable list of <see cref="PipelineStep"/> instances. It exposes methods
    /// like <see cref="AddAdvices"/>, <see cref="AddAspectInstances"/> or <see cref="AddAspectSources"/> that
    /// allow to add inputs to different steps of the pipeline. This object must create the steps in the appropriate order.
    /// </summary>
    internal class PipelineStepsState : IPipelineStepsResult, IDiagnosticAdder
    {
        private readonly SkipListDictionary<PipelineStepId, PipelineStep> _steps;
        private readonly PipelineStepIdComparer _comparer;
        private readonly UserDiagnosticSink _diagnostics;
        private readonly List<ITransformation> _transformations = new();
        private readonly List<IAspectInstance> _inheritableAspectInstances = new();
        private readonly List<AspectInstanceResult> _aspectInstanceResults = new();
        private readonly List<IValidatorSource> _validatorSources = new();
        private readonly OverflowAspectSource _overflowAspectSource = new();

        private PipelineStep? _currentStep;

        public CompilationModel LastCompilation { get; private set; }

        public ImmutableArray<CompilationModel> Compilations { get; private set; }

        public IReadOnlyList<ITransformation> Transformations => this._transformations;

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
            AspectPipelineConfiguration pipelineConfiguration )
        {
            pipelineConfiguration.ServiceProvider.GetService<IntrospectionPipelineListener>();

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
                if ( aspectLayer.AspectLayerId.IsDefault )
                {
                    var step = new EvaluateAspectSourcesPipelineStep( this, aspectLayer );

                    _ = this._steps.Add( step.Id, step );
                }
            }

            // Add the initial sources.
            // TODO: process failure of the next line.
            this.AddAspectSources( inputAspectSources );
            this.AddValidatorSources( inputValidatorSources );
        }

        public void Execute( CancellationToken cancellationToken )
        {
            using var enumerator = this._steps.GetEnumerator();
            PipelineStep? previousStep = null;

            while ( enumerator.MoveNext() )
            {
                cancellationToken.ThrowIfCancellationRequested();

                this._currentStep = enumerator.Current.Value;

                this.DetectUnorderedSteps( ref previousStep, this._currentStep );

                var compilation = this.LastCompilation.GetCompilationModel();

                this.LastCompilation = this._currentStep!.Execute( compilation, cancellationToken );

                if ( compilation != this.LastCompilation )
                {
                    this.Compilations = this.Compilations.Add( this.LastCompilation );
                }
            }
        }

        private void DetectUnorderedSteps( ref PipelineStep? previousStep, PipelineStep currentStep )
        {
            if ( previousStep != null )
            {
                if ( previousStep.AspectLayer != currentStep.AspectLayer && previousStep.AspectLayer.Order >= currentStep.AspectLayer.Order )
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

        public bool AddAspectSources( IEnumerable<IAspectSource> aspectSources )
        {
            var success = true;

            foreach ( var aspectSource in aspectSources )
            {
                foreach ( var aspectType in aspectSource.AspectClasses )
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

            return success;
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
                step = new ExecuteAspectLayerPipelineStep( this, stepId, aspectLayer );
                _ = this._steps.Add( stepId, step );
            }

            return true;
        }

        public void AddAspectInstances( IEnumerable<ResolvedAspectInstance> aspectInstances )
        {
            foreach ( var aspectInstance in aspectInstances )
            {
                var aspectTargetDeclaration = (IDeclaration) aspectInstance.TargetDeclaration;
                var aspectTargetTypeDeclaration = aspectTargetDeclaration.GetDeclaringType() ?? aspectTargetDeclaration;

                var stepId = new PipelineStepId(
                    new AspectLayerId( aspectInstance.AspectInstance.AspectClass ),
                    this.LastCompilation.GetDepth( aspectTargetTypeDeclaration ),
                    this.LastCompilation.GetDepth( aspectTargetDeclaration ),
                    PipelineStepPhase.Initialize,
                    -1 );

                if ( !this.TryGetOrAddStep(
                        stepId,
                        true,
                        out var step ) )
                {
                    // This should not happen here. The source should not have been added.
                    throw new AssertionFailedException();
                }

                ((ExecuteAspectLayerPipelineStep) step).AddAspectInstance( aspectInstance );
            }
        }

        public void AddInheritableAspectInstances( IReadOnlyList<AspectInstance> inheritedAspectInstances )
        {
            this._inheritableAspectInstances.AddRange( inheritedAspectInstances );
        }

        public void AddDiagnostics( ImmutableUserDiagnosticList diagnostics )
        {
            this._diagnostics.Report( diagnostics.ReportedDiagnostics );
            this._diagnostics.Suppress( diagnostics.DiagnosticSuppressions );
            this._diagnostics.AddCodeFixes( diagnostics.CodeFixes );
        }

        public void AddTransformations( IEnumerable<ITransformation> transformations ) => this._transformations.AddRange( transformations );

        public bool AddValidatorSources( IEnumerable<IValidatorSource> validatorSources )
        {
            this._validatorSources.AddRange( validatorSources );

            return true;
        }

        public void Report( Diagnostic diagnostic ) => this._diagnostics.Report( diagnostic );

        public void AddAspectInstanceResults( ImmutableArray<AspectInstanceResult> aspectInstanceResults )
        {
            this._aspectInstanceResults.AddRange( aspectInstanceResults );
        }
    }
}