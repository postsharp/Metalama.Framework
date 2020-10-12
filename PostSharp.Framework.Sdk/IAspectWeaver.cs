using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace PostSharp.Framework.Sdk
{
    public interface IAspectWeaver : IAspectDriver
    {
        Compilation Transform(AspectWeaverContext context);
    }

    public class AspectWeaverContext
    {
        public INamedTypeSymbol AspectType { get; }
        public IReadOnlyList<AspectInstance> AspectInstances { get; }
        public Compilation Compilation { get; }
        // TODO: diagnostics
    }
}
