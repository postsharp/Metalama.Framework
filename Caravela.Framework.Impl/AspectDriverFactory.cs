using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl
{
    class AspectDriverFactory
    {
        private readonly CSharpCompilation compilation;
        private readonly Loader loader;

        public AspectDriverFactory(CSharpCompilation compilation, Loader loader)
        {
            this.compilation = compilation;
            this.loader = loader;
        }

        public IAspectDriver GetAspectDriver(INamedType type)
        {
            var aspectWeaverAttributeType = compilation.GetTypeByMetadataName(typeof(AspectWeaverAttribute).FullName);

            var typeSymbol = ((NamedType)type).NamedTypeSymbol;

            // TODO: is ContainingAssembly enough?
            // TODO: perf
            // TODO: nested types
            var weavers =
                (from weaverType in typeSymbol.ContainingAssembly.GetTypes()
                 from attribute in weaverType.GetAttributes()
                 where attribute.AttributeClass!.Equals(aspectWeaverAttributeType, SymbolEqualityComparer.Default)
                 let targetType = (ITypeSymbol)attribute.ConstructorArguments.Single().Value!
                 where targetType.Equals(typeSymbol, SymbolEqualityComparer.Default)
                 select weaverType).ToList();

            if (weavers.Count > 1)
                throw new InvalidOperationException("There can be at most one weaver for an aspect type.");

            if (weavers.Count == 1)
                return (IAspectDriver)loader.CreateInstance(weavers[0]);

            throw new NotImplementedException();
        }
    }
}
