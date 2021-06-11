// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Maintains the state of <see cref="HighLevelPipelineStage"/>, composed of several <see cref="PipelineStep"/>.
    /// The current object is essentially a mutable list of <see cref="PipelineStep"/> instances. It exposes methods
    /// like <see cref="AddAdvices"/>, <see cref="AddAspectInstances"/> or <see cref="AddAspectSources"/> that
    /// allow to add inputs to different steps of the pipeline. This object must create the steps in the appropriate order.
    /// </summary>
    internal class PipelineStepsState : IPipelineStepsResult, IDiagnosticAdder
    {
        private readonly SkipListIndexedDictionary<PipelineStepId, PipelineStep> _steps;
        private readonly PipelineStepIdComparer _comparer;
        private readonly UserDiagnosticSink _diagnostics;
        private readonly List<INonObservableTransformation> _nonObservableTransformations = new();
        private readonly OverflowAspectSource _overflowAspectSource = new();
        private PipelineStep? _currentStep;

        public CompilationModel Compilation { get; private set; }

        public IReadOnlyList<INonObservableTransformation> NonObservableTransformations => this._nonObservableTransformations;

        public ImmutableUserDiagnosticList Diagnostics => this._diagnostics.ToImmutable();

        public IDiagnosticAdder DiagnosticAdder => this._diagnostics;

        public IReadOnlyList<IAspectSource> ExternalAspectSources => new[] { this._overflowAspectSource };

        public PipelineStepsState(
            IReadOnlyList<OrderedAspectLayer> aspectLayers,
            CompilationModel inputCompilation,
            CompileTimeProject compileTimeProject,
            IReadOnlyList<IAspectSource> inputAspectSources )
        {
            this._diagnostics = new UserDiagnosticSink( compileTimeProject );
            this.Compilation = inputCompilation;

            // Create an empty collection of steps.
            this._comparer = new PipelineStepIdComparer( aspectLayers );
            this._steps = new SkipListIndexedDictionary<PipelineStepId, PipelineStep>( this._comparer );

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

            while ( enumerator.MoveNext() )
            {
                cancellationToken.ThrowIfCancellationRequested();

                this._currentStep = enumerator.Current.Value;
                var compilationWithAspectLayerId = this.Compilation.GetCompilationModel().WithAspectLayerId( this._currentStep.AspectLayer.AspectLayerId );
                this.Compilation = this._currentStep!.Execute( compilationWithAspectLayerId, this, cancellationToken );
            }
        }

        public bool AddAspectSources( IEnumerable<IAspectSource> aspectSources )
        {
            var success = true;

            foreach ( var aspectSource in aspectSources )
            {
                foreach ( var aspectType in aspectSource.AspectTypes )
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
                                    (this._currentStep.AspectLayer.AspectClass.DisplayName, aspectType.DisplayName) ) );

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
                var depth = this.Compilation.GetDepth( advice.TargetDeclaration );

                if ( !this.TryGetOrAddStep( advice.AspectLayerId, depth, true, out var step ) )
                {
                    this._diagnostics.Report(
                        GeneralDiagnosticDescriptors.CannotAddAdviceToPreviousPipelineStep.CreateDiagnostic(
                            this._currentStep.AspectLayer.AspectClass.DiagnosticLocation,
                            (this._currentStep.AspectLayer.AspectClass.DisplayName, advice.TargetDeclaration) ) );

                    success = false;

                    continue;
                }

                ((AdvicePipelineStep) step).AddAdvice( advice );
            }

            return success;
        }

        public void AddAspectInstances( IEnumerable<AspectInstance> aspectInstances )
        {
            foreach ( var aspectInstance in aspectInstances )
            {
                var depth = this.Compilation.GetDepth( aspectInstance.TargetDeclaration );

                if ( !this.TryGetOrAddStep( new AspectLayerId( aspectInstance.AspectClass ), depth, true, out var step ) )
                {
                    // This should not happen here. The source should not have been added.
                    throw new AssertionFailedException();
                }

                ((InitializeAspectInstancesPipelineStep) step).AddAspectInstance( aspectInstance );
            }
        }

        public void AddDiagnostics( IEnumerable<Diagnostic> diagnostics, IEnumerable<ScopedSuppression> suppressions )
        {
            this._diagnostics.Report( diagnostics );
            this._diagnostics.Suppress( suppressions );
        }

        public void AddNonObservableTransformations( IEnumerable<INonObservableTransformation> transformations )
            => this._nonObservableTransformations.AddRange( transformations );

        public void Report( Diagnostic diagnostic ) => this._diagnostics.Report( diagnostic );
    }
}