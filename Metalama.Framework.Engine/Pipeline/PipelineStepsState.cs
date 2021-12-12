// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Validation;
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
        private readonly List<INonObservableTransformation> _nonObservableTransformations = new();
        private readonly List<IAspectInstance> _inheritableAspectInstances = new();
        private readonly List<ValidatorSource> _validatorSources = new();
        private readonly OverflowAspectSource _overflowAspectSource = new();
        private PipelineStep? _currentStep;

        public CompilationModel LastCompilation { get; private set; }
        
        public ImmutableArray<CompilationModel> Compilations { get; private set; }

        public IReadOnlyList<INonObservableTransformation> NonObservableTransformations => this._nonObservableTransformations;

        public ImmutableArray<IAspectInstance> InheritableAspectInstances => this._inheritableAspectInstances.ToImmutableArray();

        public ImmutableArray<ValidatorSource> ValidatorSources => this._validatorSources.ToImmutableArray();

        public ImmutableUserDiagnosticList Diagnostics => this._diagnostics.ToImmutable();

        public ImmutableArray<IAspectSource> ExternalAspectSources => ImmutableArray.Create<IAspectSource>( this._overflowAspectSource );

        public AspectPipelineConfiguration PipelineConfiguration { get; }

        public PipelineStepsState(
            IReadOnlyList<OrderedAspectLayer> aspectLayers,
            CompilationModel inputLastCompilation,
            IReadOnlyList<IAspectSource> inputAspectSources,
            AspectPipelineConfiguration pipelineConfiguration )
        {
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
                    var step = new EvaluateAspectSourcesPipelineStep( aspectLayer );

                    _ = this._steps.Add( step.Id, step );
                }
            }

            // Add the initial sources.
            // TODO: process failure of the next line.
            this.AddAspectSources( inputAspectSources );
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

                this.LastCompilation = this._currentStep!.Execute( compilation, this, cancellationToken );

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
                        GeneralDiagnosticDescriptors.UnorderedLayers.CreateDiagnostic(
                            null,
                            (previousStep.AspectLayer.AspectLayerId.ToString(), currentStep.AspectLayer.AspectLayerId.ToString()) ) );
                }

                if ( previousStep.AspectLayer == currentStep.AspectLayer && previousStep.Id.Depth >= currentStep.Id.Depth )
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
                        if ( !this.TryGetOrAddStep( aspectLayerId, -1, false, out var step ) )
                        {
                            this._diagnostics.Report(
                                GeneralDiagnosticDescriptors.CannotAddChildAspectToPreviousPipelineStep.CreateDiagnostic(
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

        private bool TryGetOrAddStep( AspectLayerId aspectLayerId, int depth, bool allowAddToCurrentLayer, [NotNullWhen( true )] out PipelineStep? step )
        {
            var stepId = new PipelineStepId( aspectLayerId, depth );

            var aspectLayer = this._comparer.GetOrderedAspectLayer( aspectLayerId );

            if ( this._currentStep != null )
            {
                var currentLayerOrder = this._currentStep.AspectLayer.Order;

                if ( aspectLayer.Order < currentLayerOrder || (!allowAddToCurrentLayer && aspectLayer.Order == currentLayerOrder) )
                {
                    // Cannot add a step before the current one.
                    step = null;

                    return false;
                }
            }

            if ( !this._steps.TryGetValue( stepId, out step ) )
            {
                if ( aspectLayer.IsDefault )
                {
                    step = new InitializeAspectInstancesPipelineStep( stepId, aspectLayer );
                }
                else
                {
                    step = new AdvicePipelineStep( stepId, aspectLayer );
                }

                _ = this._steps.Add( stepId, step );
            }

            return true;
        }

        public bool AddAdvices( IEnumerable<Advice> advices )
        {
            Invariant.Assert( this._currentStep != null );

            var success = true;

            foreach ( var advice in advices )
            {
                var depth = this.LastCompilation.GetDepth( advice.TargetDeclaration );

                if ( !this.TryGetOrAddStep( advice.AspectLayerId, depth, true, out var step ) )
                {
                    this._diagnostics.Report(
                        GeneralDiagnosticDescriptors.CannotAddAdviceToPreviousPipelineStep.CreateDiagnostic(
                            this._currentStep.AspectLayer.AspectClass.DiagnosticLocation,
                            (this._currentStep.AspectLayer.AspectClass.ShortName, advice.TargetDeclaration) ) );

                    success = false;

                    continue;
                }

                ((AdvicePipelineStep) step).AddAdvice( advice );
            }

            return success;
        }

        public void AddAspectInstances( IEnumerable<ResolvedAspectInstance> aspectInstances )
        {
            foreach ( var aspectInstance in aspectInstances )
            {
                var depth = this.LastCompilation.GetDepth( aspectInstance.TargetDeclaration );

                if ( !this.TryGetOrAddStep( new AspectLayerId( aspectInstance.AspectInstance.AspectClass ), depth, true, out var step ) )
                {
                    // This should not happen here. The source should not have been added.
                    throw new AssertionFailedException();
                }

                ((InitializeAspectInstancesPipelineStep) step).AddAspectInstance( aspectInstance );
            }
        }

        public void AddInheritableAspectInstances( IReadOnlyList<AspectInstance> inheritedAspectInstances )
        {
            this._inheritableAspectInstances.AddRange( inheritedAspectInstances );
        }

        public void AddDiagnostics(
            IEnumerable<Diagnostic> diagnostics,
            IEnumerable<ScopedSuppression> suppressions,
            IEnumerable<CodeFixInstance> codeFixInstances )
        {
            this._diagnostics.Report( diagnostics );
            this._diagnostics.Suppress( suppressions );
            this._diagnostics.AddCodeFixes( codeFixInstances );
        }

        public void AddNonObservableTransformations( IEnumerable<INonObservableTransformation> transformations )
            => this._nonObservableTransformations.AddRange( transformations );

        public bool AddValidatorSources( IEnumerable<ValidatorSource> validatorSources )
        {
            this._validatorSources.AddRange( validatorSources );

            return true;
        }

        public void Report( Diagnostic diagnostic ) => this._diagnostics.Report( diagnostic );
    }
}