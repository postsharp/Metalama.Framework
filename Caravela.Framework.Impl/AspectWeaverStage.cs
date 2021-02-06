using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl
{
    internal sealed class AspectWeaverStage : PipelineStage
    {
        private readonly IAspectWeaver aspectWeaver;
        private readonly INamedType aspectType;

        public AspectWeaverStage( IAspectWeaver aspectWeaver, INamedType aspectType )
        {
            this.aspectWeaver = aspectWeaver;
            this.aspectType = aspectType;
        }

        public override PipelineStageResult ToResult( PipelineStageResult input )
        {
            // TODO: actual reactivity.
            var aspectInstances = ((IList<AspectInstance>) input.AspectInstances.Where( x => x.AspectType.FullName == this.aspectType.FullName )).ToImmutableArray();

            if ( !aspectInstances.Any() )
            {
                return input;
            }

            var diagnosticSink = new DiagnosticSink();

            var resources = new List<ResourceDescription>();

            var context = new AspectWeaverContext(
                this.aspectType, aspectInstances, input.Compilation.GetRoslynCompilation(), diagnosticSink, resources.Add );

            CSharpCompilation newCompilation;
            try
            {
                newCompilation = this.aspectWeaver.Transform( context );
            }
            catch ( Exception ex )
            {
                newCompilation = context.Compilation;
                diagnosticSink.AddDiagnostic( Diagnostic.Create( GeneralDiagnosticDescriptors.ExceptionInWeaver, null, this.aspectType, ex.ToDiagnosticString() ) );
            }

            // TODO: update AspectCompilation.Aspects
            return new PipelineStageResult(
                new SourceCompilationModel( newCompilation ),
                input.Diagnostics.Concat( diagnosticSink.Diagnostics ).ToList(),
                input.Resources.Concat( resources ).ToList(),
                input.AspectInstances );
        }

        private class DiagnosticSink : IDiagnosticSink
        {
            public List<Diagnostic> Diagnostics { get; } = new ();

            public void AddDiagnostic( Diagnostic diagnostic ) => this.Diagnostics.Add( diagnostic );
        }
    }
}
