using System.Collections.Immutable;
using Caravela.Framework.Aspects;
using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    class AttributeAspectSource : AspectSource
    {
        private readonly ICompilation compilation;
        private readonly AspectTypeFactory aspectTypeFactory;

        public AttributeAspectSource(ICompilation compilation, AspectTypeFactory aspectTypeFactory)
        {
            this.compilation = compilation;
            this.aspectTypeFactory = aspectTypeFactory;
        }

        public override IReactiveCollection<AspectInstance> GetAspects()
        {
            var results = ImmutableArray.CreateBuilder<AspectInstance>();

            var iAspect = compilation.GetTypeByMetadataName(typeof(IAspect).FullName)!;

            return from type in compilation.Types
                   from attribute in type.Attributes
                   where attribute.Type.Is(iAspect)
                   let aspectType = aspectTypeFactory.GetAspectType(attribute.Type)
                   select new AspectInstance(null!, type, aspectType);
        }
    }
}
