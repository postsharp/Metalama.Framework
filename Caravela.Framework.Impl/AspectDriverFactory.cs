using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Caravela.Framework.Sdk;
using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    class AspectDriverFactory
    {
        private readonly Loader loader;
        private readonly IReactiveGroupBy<IType, ITypeInfo> weaverTypes;

        public AspectDriverFactory(ICompilation compilation, Loader loader)
        {
            this.loader = loader;

            var aspectWeaverAttributeType = compilation.GetTypeByMetadataName(typeof(AspectWeaverAttribute).FullName)!;

            // TODO: nested types?
            this.weaverTypes =
                from weaverType in compilation.Types
                from attribute in weaverType.Attributes
                where attribute.Type.Is(aspectWeaverAttributeType)
                group weaverType by (IType)attribute.ConstructorArguments.Single()!;
        }

        public IAspectDriver GetAspectDriver(INamedType type, in ReactiveObserverToken observerToken)
        {
            var weavers = weaverTypes[type].GetValue(observerToken).ToList();

            if (weavers.Count > 1)
                throw new InvalidOperationException("There can be at most one weaver for an aspect type.");

            if (weavers.Count == 1)
                return (IAspectDriver)loader.CreateInstance(((TypeInfo)weavers.Single()).TypeSymbol);

            throw new NotImplementedException();
        }
    }
}
