using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using PostSharp.Framework.Sdk;

namespace PostSharp.Framework.Impl
{
    class AspectDriverFactory
    {
        private readonly CSharpCompilation compilation;
        private readonly Loader loader;

        public AspectDriverFactory(CSharpCompilation compilation)
        {
            this.compilation = compilation;
            loader = new Loader(compilation);
        }

        public IAspectDriver GetAspectDriver(INamedType type)
        {
            var aspectWeaverAttributeType = compilation.GetTypeByMetadataName(typeof(AspectWeaverAttribute).FullName);

            var typeSymbol = ((NamedType)type).NamedTypeSymbol;

            // TODO: it would be easier, if the attribute was on the aspect pointing to the weaver
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
