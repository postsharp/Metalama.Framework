using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Sdk
{
    public interface IAspectWeaver : IAspectDriver
    {
        CSharpCompilation Transform(AspectWeaverContext context);
    }

    public class AspectWeaverContext
    {
        public INamedTypeSymbol AspectType { get; }
        public IReadOnlyList<AspectInstance> AspectInstances { get; }
        public CSharpCompilation Compilation { get; }
        public IDiagnosticSink Diagnostics { get; }

        internal AspectWeaverContext(INamedTypeSymbol aspectType, IReadOnlyList<AspectInstance> aspectInstances, CSharpCompilation compilation, IDiagnosticSink diagnostics)
        {
            AspectType = aspectType;
            AspectInstances = aspectInstances;
            Compilation = compilation;
            Diagnostics = diagnostics;
        }
    }

    public interface IDiagnosticSink
    {
        void AddDiagnostic(Diagnostic diagnostic);
    }
}
