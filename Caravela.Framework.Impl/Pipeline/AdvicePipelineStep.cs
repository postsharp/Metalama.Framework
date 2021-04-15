// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A <see cref="PipelineStep"/> that can contain advices.
    /// </summary>
    internal class AdvicePipelineStep : PipelineStep
    {
        private readonly List<Advice> _advices = new();

        public AdvicePipelineStep( PipelineStepId id, OrderedAspectLayer aspectLayer ) : base( id, aspectLayer ) { }

        public void AddAdvice( Advice advice ) => this._advices.Add( advice );

        public override CompilationModel Execute( CompilationModel compilation, PipelineStepsState pipelineStepsState )
        {
            var adviceResults = this._advices
                                    .Select( ai => ai.ToResult( compilation ) )
                                    .ToList();

            var addedObservableIntroductions = adviceResults.SelectMany( ar => ar.ObservableTransformations ).ToReadOnlyList();
            var addedNonObservableTransformations = adviceResults.SelectMany( ar => ar.NonObservableTransformations );

            pipelineStepsState.AddNonObservableTransformations( addedNonObservableTransformations );

            return CompilationModel.CreateRevisedInstance( compilation, addedObservableIntroductions );
        }
    }
}