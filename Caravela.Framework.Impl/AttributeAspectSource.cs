using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Impl
{
    class AttributeAspectSource : AspectSource
    {
        private readonly CSharpCompilation compilation;
        private readonly AspectTypeFactory aspectTypeFactory;

        public AttributeAspectSource(CSharpCompilation compilation, AspectTypeFactory aspectTypeFactory)
        {
            this.compilation = compilation;
            this.aspectTypeFactory = aspectTypeFactory;
        }

        public override IReadOnlyList<AspectInstance> GetAspects()
        {
            var results = ImmutableArray.CreateBuilder<AspectInstance>();

            var caravelaCompilation = new Compilation(compilation);
            var iAspect = caravelaCompilation.GetTypeByMetadataName(typeof(IAspect).FullName)!;

            foreach (var type in caravelaCompilation.Types)
            {
                foreach (var attribute in type.Attributes)
                {
                    if (attribute.Type.Is(iAspect))
                    {
                        var aspectType = aspectTypeFactory.GetAspectType(attribute.Type);

                        // TODO: create the aspect
                        results.Add(new AspectInstance(null!, type, aspectType));
                    }
                }
            }

            return results.ToImmutable();
        }
    }
}
