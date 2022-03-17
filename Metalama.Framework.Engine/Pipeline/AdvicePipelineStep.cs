// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CodeFixes;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Microsoft.CodeAnalysis;
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

        public void AddAdvice( Advice advice )
        {
            advice.Order = this._advices.Count + 1;
            this._advices.Add( advice );
        }

        public override CompilationModel Execute(
            CompilationModel compilation,
            PipelineStepsState pipelineStepsState,
            CancellationToken cancellationToken )
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ( this._advices.Count == 0 )
            {
                // Nothing to do, so don't allocate memory.

                return compilation;
            }

            var observableTransformations = new List<IObservableTransformation>();
            var nonObservableTransformations = new List<INonObservableTransformation>();
            var diagnostics = new List<Diagnostic>();

            foreach ( var advice in this._advices )
            {
                var result = advice.ToResult( compilation, observableTransformations );

                foreach ( var transformation in result.ObservableTransformations )
                {
                    observableTransformations.Add( transformation );
                }

                foreach ( var transformation in result.NonObservableTransformations )
                {
                    nonObservableTransformations.Add( transformation );
                }

                diagnostics.AddRange( result.Diagnostics );
            }

            pipelineStepsState.AddNonObservableTransformations( nonObservableTransformations );
            pipelineStepsState.AddDiagnostics( diagnostics, Enumerable.Empty<ScopedSuppression>(), Enumerable.Empty<CodeFixInstance>() );

            return compilation.WithTransformations( observableTransformations );
        }
    }
}