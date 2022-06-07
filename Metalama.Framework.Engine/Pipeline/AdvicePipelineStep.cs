// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
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

        public AdvicePipelineStep( PipelineStepsState parent, PipelineStepId id, OrderedAspectLayer aspectLayer ) : base( parent, id, aspectLayer ) { }

        public void AddAdvice( Advice advice )
        {
            advice.Order = this._advices.Count + 1;
            this._advices.Add( advice );
        }

        public override CompilationModel Execute(
            CompilationModel compilation,
            CancellationToken cancellationToken )
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ( this._advices.Count == 0 )
            {
                // Nothing to do, so don't allocate memory.

                return compilation;
            }

            var observableTransformations = new List<IObservableTransformation>();
            var transformations = new List<ITransformation>();
            var diagnostics = new List<Diagnostic>();

            foreach ( var adviceGroup in this._advices.GroupBy( a => a.TargetDeclaration.GetTarget( compilation ).GetDeclaringType() ) )
            {
                var compilationForThisType = compilation;
                var groupCount = adviceGroup.Count();

                foreach ( var advice in adviceGroup.OrderBy( a => a.Order ) )
                {
                    var result = advice.ToResult( this.Parent.PipelineConfiguration.ServiceProvider, compilationForThisType );

                    foreach ( var transformation in result.ObservableTransformations )
                    {
                        observableTransformations.Add( transformation );
                        transformations.Add( transformation );

                        // If we have several observable transformations, update the component model within the same type.
                        if ( groupCount > 1 )
                        {
                            if ( !compilationForThisType.IsMutable )
                            {
                                compilationForThisType = compilationForThisType.ToMutable();
                            }

                            compilationForThisType.AddTransformation( transformation );
                        }
                    }

                    foreach ( var transformation in result.NonObservableTransformations )
                    {
                        transformations.Add( transformation );
                    }

                    diagnostics.AddRange( result.Diagnostics );

                    // Add the result for introspection.
                    this.Parent.AddAdviceResult( advice, result );
                }
            }

            this.Parent.AddTransformations( transformations );
            this.Parent.AddDiagnostics( diagnostics, Enumerable.Empty<ScopedSuppression>(), Enumerable.Empty<CodeFixInstance>() );

            return compilation.WithTransformations( observableTransformations );
        }
    }
}