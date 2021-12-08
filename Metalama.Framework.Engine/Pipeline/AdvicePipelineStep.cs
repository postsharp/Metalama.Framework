// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.DesignTime.CodeFixes;
using Metalama.Framework.Engine.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// A <see cref="PipelineStep"/> that can contain advices.
    /// </summary>
    internal class AdvicePipelineStep : PipelineStep
    {
        private readonly List<Advice> _advices = new();

        public AdvicePipelineStep( PipelineStepId id, OrderedAspectLayer aspectLayer ) : base( id, aspectLayer ) { }

        public void AddAdvice( Advice advice ) => this._advices.Add( advice );

        public override CompilationModel Execute(
            CompilationModel compilation,
            PipelineStepsState pipelineStepsState,
            CancellationToken cancellationToken )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var adviceResults = this._advices
                .Select( ai => ai.ToResult( compilation ) )
                .ToList();

            var addedObservableIntroductions = adviceResults.SelectMany( ar => ar.ObservableTransformations ).ToReadOnlyList();
            var addedNonObservableTransformations = adviceResults.SelectMany( ar => ar.NonObservableTransformations );
            var addedDiagnostics = adviceResults.SelectMany( ar => ar.Diagnostics );

            pipelineStepsState.AddNonObservableTransformations( addedNonObservableTransformations );
            pipelineStepsState.AddDiagnostics( addedDiagnostics, Enumerable.Empty<ScopedSuppression>(), Enumerable.Empty<CodeFixInstance>() );

            return compilation.WithTransformations( addedObservableIntroductions );
        }
    }
}