using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Sdk
{
    [CompileTime]
    public interface IAspectWeaver : IAspectDriver
    {
        CSharpCompilation Transform(AspectWeaverContext context);
    }

    public class AspectWeaverContext
    {
        public INamedType AspectType { get; }
        public IReadOnlyList<AspectInstance> AspectInstances { get; }
        public CSharpCompilation Compilation { get; }
        public IDiagnosticSink Diagnostics { get; }

        internal AspectWeaverContext(INamedType aspectType, IReadOnlyList<AspectInstance> aspectInstances, CSharpCompilation compilation, IDiagnosticSink diagnostics)
        {
            this.AspectType = aspectType;
            this.AspectInstances = aspectInstances;
            this.Compilation = compilation;
            this.Diagnostics = diagnostics;
        }
    }

    public interface IDiagnosticSink
    {
        void AddDiagnostic(Diagnostic diagnostic);
    }
}
