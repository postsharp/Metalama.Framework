using System.Collections.Generic;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    sealed class AspectWeaverStage : PipelineStage
    {
        private readonly IAspectWeaver aspectWeaver;
        private readonly INamedTypeSymbol aspectType;
        private readonly IReadOnlyList<AspectInstance> aspectInstances;

        public AspectWeaverStage(IAspectWeaver aspectWeaver, INamedTypeSymbol aspectType, IReadOnlyList<AspectInstance> aspectInstances)
        {
            this.aspectWeaver = aspectWeaver;
            this.aspectType = aspectType;
            this.aspectInstances = aspectInstances;
        }

        public override AspectCompilation Transform(AspectCompilation input)
        {
            var diagnosticSink = new DiagnosticSink();

            var newCompilation = this.aspectWeaver.Transform(
                new AspectWeaverContext( this.aspectType, this.aspectInstances, ((Compilation)input.Compilation).RoslynCompilation, diagnosticSink));

            return input.Update(diagnosticSink.Diagnostics, new Compilation(newCompilation));
        }

        class DiagnosticSink : IDiagnosticSink
        {
            public List<Diagnostic> Diagnostics { get; } = new();

            public void AddDiagnostic(Diagnostic diagnostic) => this.Diagnostics.Add(diagnostic);
        }
    }
}
