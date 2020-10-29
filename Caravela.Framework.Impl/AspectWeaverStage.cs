using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

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
            var aspectInstances = input.AspectsByAspectType[this.aspectType.Name].GetValue().ToImmutableArray();

            if ( aspectInstances.IsEmpty )
                return input;

            var diagnosticSink = new DiagnosticSink();

            var newCompilation = this.aspectWeaver.Transform(
                new AspectWeaverContext( this.aspectType, aspectInstances, input.Compilation.GetRoslynCompilation(), diagnosticSink ) );

            // TODO: update AspectCompilation.Aspects
            return input.Update(diagnosticSink.Diagnostics, new SourceCompilation(newCompilation));
        }

        class DiagnosticSink : IDiagnosticSink
        {
            public List<Diagnostic> Diagnostics { get; } = new();

            public void AddDiagnostic(Diagnostic diagnostic) => this.Diagnostics.Add(diagnostic);
        }
    }
}
