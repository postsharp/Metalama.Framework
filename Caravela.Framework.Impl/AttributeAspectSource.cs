using System.Collections.Immutable;
using Caravela.Framework.Aspects;
using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    class AttributeAspectSource : AspectSource
    {
        private readonly ICompilation compilation;

        public AttributeAspectSource(ICompilation compilation)
        {
            this.compilation = compilation;
        }

        public override IReactiveCollection<AspectInstance> GetAspects()
        {
            var results = ImmutableArray.CreateBuilder<AspectInstance>();

            var iAspect = compilation.GetTypeByMetadataName(typeof(IAspect).FullName)!;

            return from type in compilation.Types
                   from attribute in type.Attributes
                   where attribute.Type.Is(iAspect)
                   select new AspectInstance(null!, type, attribute.Type);
        }
    }
}
