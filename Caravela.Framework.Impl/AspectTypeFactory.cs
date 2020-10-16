using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    class AspectTypeFactory
    {
        private readonly AspectDriverFactory aspectDriverFactory;

        private readonly Dictionary<INamedType, AspectType> aspectTypes = new();

        public AspectTypeFactory(AspectDriverFactory aspectDriverFactory) => this.aspectDriverFactory = aspectDriverFactory;

        public AspectType GetAspectType(INamedType attributeType, in ReactiveCollectorToken collectorToken)
        {
            if (!aspectTypes.TryGetValue(attributeType, out var aspectType))
            {
                // TODO: handle AspectParts properly
                aspectType = new AspectType(
                    attributeType.FullName, aspectDriverFactory.GetAspectDriver(attributeType, collectorToken), ImmutableArray.Create(new AspectPart(null, 0)));
                aspectTypes.Add(attributeType, aspectType);
            }

            return aspectType;
        }
    }
}
