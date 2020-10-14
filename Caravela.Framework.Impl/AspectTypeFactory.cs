using System.Collections.Generic;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl
{
    class AspectTypeFactory
    {
        private readonly AspectDriverFactory aspectDriverFactory;

        private readonly Dictionary<INamedType, AspectType> aspectTypes = new();

        public AspectTypeFactory(AspectDriverFactory aspectDriverFactory) => this.aspectDriverFactory = aspectDriverFactory;

        public AspectType GetAspectType(INamedType attributeType)
        {
            if (!aspectTypes.TryGetValue(attributeType, out var aspectType))
            {
                // TODO: handle AspectParts properly
                aspectType = new AspectType(attributeType.FullName, aspectDriverFactory.GetAspectDriver(attributeType), new[] { new AspectPart(null, 0) });
                aspectTypes.Add(attributeType, aspectType);
            }

            return aspectType;
        }
    }
}
