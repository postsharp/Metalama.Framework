using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A <see cref="PipelineStage"/> that has a single aspect backed by a low-level <see cref="IAspectWeaver"/>.
    /// </summary>
    internal sealed class LowLevelAspectsPipelineStage : PipelineStage
    {
        private readonly IAspectWeaver _aspectWeaver;
        private readonly INamedType _aspectType;

        public LowLevelAspectsPipelineStage( IAspectWeaver aspectWeaver, INamedType aspectType )
        {
            this._aspectWeaver = aspectWeaver;
            this._aspectType = aspectType;
        }

        /// <inheritdoc/>
        public override PipelineStageResult ToResult( PipelineStageResult input )
        {
            var aspectInstances = input.AspectSources.SelectMany( s => s.GetAspectInstances( this._aspectType ) ).ToImmutableArray();

            if ( !aspectInstances.Any() )
            {
                return input;
            }

            var diagnosticSink = new DiagnosticSink();

            var resources = new List<ResourceDescription>();

            var context = new AspectWeaverContext(
                this._aspectType, aspectInstances, input.Compilation, diagnosticSink, resources.Add );

            CSharpCompilation newCompilation;
            try
            {
                newCompilation = this._aspectWeaver.Transform( context );
            }
            catch ( Exception ex )
            {
                newCompilation = context.Compilation;
                diagnosticSink.AddDiagnostic( Diagnostic.Create( GeneralDiagnosticDescriptors.ExceptionInWeaver, null, this._aspectType, ex.ToDiagnosticString() ) );
            }

            // TODO: update AspectCompilation.Aspects
            return new PipelineStageResult(
                newCompilation,
                input.AspectParts,
                input.Diagnostics.Concat( diagnosticSink.Diagnostics ).ToList(),
                input.Resources.Concat( resources ).ToList(),
                input.AspectSources );
        }

        private class DiagnosticSink : IDiagnosticSink
        {
            public List<Diagnostic> Diagnostics { get; } = new();

            public void AddDiagnostic( Diagnostic diagnostic ) => this.Diagnostics.Add( diagnostic );
        }
    }
}
