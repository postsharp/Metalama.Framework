using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl
{
    sealed class AspectWeaverStage : PipelineStage
    {
        private readonly IAspectWeaver aspectWeaver;
        private readonly INamedType aspectType;

        public AspectWeaverStage(IAspectWeaver aspectWeaver, INamedType aspectType)
        {
            this.aspectWeaver = aspectWeaver;
            this.aspectType = aspectType;
        }

        public override AspectCompilation Transform(AspectCompilation input)
        {
            var aspectInstances = input.AspectsByAspectType[this.aspectType.FullName].GetValue().ToImmutableList();

            if ( aspectInstances.IsEmpty )
                return input;

            var diagnosticSink = new DiagnosticSink();

            var resources = new List<ResourceDescription>();

            var context = new AspectWeaverContext(
                this.aspectType, aspectInstances, input.Compilation.GetRoslynCompilation(), diagnosticSink, resources.Add );
            CSharpCompilation newCompilation;
            try
            {
                newCompilation = this.aspectWeaver.Transform( context );
            }
            catch (Exception ex)
            {
                newCompilation = context.Compilation;
                diagnosticSink.AddDiagnostic( Diagnostic.Create( GeneralDiagnosticDescriptors.ExceptionInWeaver, null, this.aspectType, ex.ToDiagnosticString() ) );
            }

            // TODO: update AspectCompilation.Aspects
            return input.Update(diagnosticSink.Diagnostics, resources, new SourceCompilation(newCompilation));
        }

        class DiagnosticSink : IDiagnosticSink
        {
            public List<Diagnostic> Diagnostics { get; } = new();

            public void AddDiagnostic(Diagnostic diagnostic) => this.Diagnostics.Add(diagnostic);
        }
    }
}
