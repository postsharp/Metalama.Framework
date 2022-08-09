// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Transformations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline;

/// <summary>
/// The <see cref="PipelineStep"/> that runs the default layer of each aspect. It runs the aspect initializer method.
/// </summary>
internal class ExecuteAspectLayerPipelineStep : PipelineStep
{
    private readonly List<AspectInstance> _aspectInstances = new();

    public ExecuteAspectLayerPipelineStep( PipelineStepsState parent, PipelineStepId stepId, OrderedAspectLayer aspectLayer ) : base(
        parent,
        stepId,
        aspectLayer ) { }

    public void AddAspectInstance( in ResolvedAspectInstance aspectInstance ) => this._aspectInstances.Add( aspectInstance.AspectInstance );

    public override CompilationModel Execute(
        CompilationModel compilation,
        CancellationToken cancellationToken )
    {
        var aggregateInstances = this._aspectInstances
            .GroupBy( a => a.TargetDeclaration )
            .Select( AggregateAspectInstance.GetInstance )
            .WhereNotNull()
            .Select( a => (TargetDeclaration: a.TargetDeclaration.GetTarget( compilation ), AspectInstance: a) );

        var instancesByType = aggregateInstances.GroupBy( a => a.TargetDeclaration.GetDeclaringType() );

        // This collection will contain the observable transformations that need to be replayed on the compilation.
        var observableTransformations = new ConcurrentQueue<IObservableTransformation>();

        // The processing order of types is arbitrary. Different types can be processed in parallel.
        foreach ( var typeGroup in instancesByType )
        {
            this.ProcessType( typeGroup, compilation, observableTransformations.Enqueue, cancellationToken );
        }

        return compilation.WithTransformations( observableTransformations );
    }

    private void ProcessType(
        IEnumerable<(IDeclaration TargetDeclaration, IAspectInstanceInternal AspectInstance)> aspects,
        CompilationModel compilation,
        Action<IObservableTransformation> addTransformation,
        CancellationToken cancellationToken )
    {
        var aspectDriver = (AspectDriver) this.AspectLayer.AspectClass.AspectDriver;

        var currentCompilation = compilation;

        var index = 0;

        foreach ( var aspect in aspects )
        {
            // Set the aspect instance order for use by the linker.
            aspect.AspectInstance.OrderWithinTypeAndAspectLayer = index;
            index++;

            // Create a snapshot of the compilation.
            var mutableCompilationForThisAspect = currentCompilation.CreateMutableClone();

            // Execute the aspect.
            var aspectResult = aspectDriver.ExecuteAspect(
                aspect.AspectInstance,
                this.Id.AspectLayerId.LayerName,
                currentCompilation,
                mutableCompilationForThisAspect,
                this.Parent.PipelineConfiguration,
                cancellationToken );

            mutableCompilationForThisAspect.Freeze();

            this.Parent.AddDiagnostics( aspectResult.Diagnostics );

            switch ( aspectResult.Outcome )
            {
                case AdviceOutcome.Error:
                case AdviceOutcome.Ignored:
                    // We roll back the changes done to the compilation model.
                    break;

                default:
                    // Apply the changes done by the aspects.
                    currentCompilation = mutableCompilationForThisAspect;

                    this.Parent.AddAspectSources( aspectResult.AspectSources );
                    this.Parent.AddValidatorSources( aspectResult.ValidatorSources );
                    this.Parent.AddTransformations( aspectResult.Transformations );

                    foreach ( var transformation in aspectResult.Transformations )
                    {
                        if ( transformation is IObservableTransformation observableTransformation )
                        {
                            addTransformation( observableTransformation );
                        }
                    }

                    break;
            }

            this.Parent.AddAspectInstanceResult( aspectResult );
        }
    }
}