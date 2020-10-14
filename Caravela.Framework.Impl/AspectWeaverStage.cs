using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Caravela.Framework.Sdk;

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

            var newCompilation = aspectWeaver.Transform(new AspectWeaverContext(aspectType, aspectInstances, input.Compilation, diagnosticSink));

            return input.Update(diagnosticSink.Diagnsotics, newCompilation);
        }

        class DiagnosticSink : IDiagnosticSink
        {
            public List<Diagnostic> Diagnsotics { get; } = new();

            public void AddDiagnostic(Diagnostic diagnostic) => Diagnsotics.Add(diagnostic);
        }
    }
}
