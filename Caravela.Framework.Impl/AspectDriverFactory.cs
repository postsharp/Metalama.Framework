using System;
using System.Linq;
using Caravela.Framework.Sdk;
using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    class AspectDriverFactory
    {
        private readonly Loader _loader;
        private readonly IReactiveGroupBy<IType, INamedType> _weaverTypes;

        public AspectDriverFactory(ICompilation compilation, Loader loader)
        {
            this._loader = loader;

            var aspectWeaverAttributeType = compilation.GetTypeByMetadataName(typeof(AspectWeaverAttribute).FullName)!;

            // TODO: nested types?
            this._weaverTypes =
                from weaverType in compilation.DeclaredAndReferencedTypes
                from attribute in weaverType.Attributes
                where attribute.Type.Is(aspectWeaverAttributeType)
                group weaverType by (IType)attribute.ConstructorArguments.Single()!;
        }

        public IAspectDriver GetAspectDriver(INamedType type, in ReactiveCollectorToken observerToken)
        {
            var weavers = this._weaverTypes[type].GetValue(observerToken).ToList();

            if (weavers.Count > 1)
                throw new InvalidOperationException("There can be at most one weaver for an aspect type.");

            if (weavers.Count == 1)
                return (IAspectDriver) this._loader.CreateInstance(((NamedType)weavers.Single()).NamedTypeSymbol);

            throw new NotImplementedException();
        }
    }
}
