using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using PostSharp.Framework.Aspects;
using PostSharp.Framework.Sdk;

namespace PostSharp.Framework.Impl
{
    class AttributeAspectSource : AspectSource
    {
        private readonly CSharpCompilation compilation;
        private readonly AspectDriverFactory aspectDriverFactory;

        public AttributeAspectSource(CSharpCompilation compilation, AspectDriverFactory aspectDriverFactory)
        {
            this.compilation = compilation;
            this.aspectDriverFactory = aspectDriverFactory;
        }

        public override IReadOnlyList<AspectInstance> GetAspects()
        {
            // TODO: this should probably be in a separate type shared by other aspect sources
            var aspectTypes = new Dictionary<INamedType, AspectType>();

            var results = ImmutableArray.CreateBuilder<AspectInstance>();

            var postSharpCompilation = new Compilation(compilation);
            var iAspect = postSharpCompilation.GetTypeByMetadataName(typeof(IAspect).FullName);

            foreach (var type in postSharpCompilation.Types)
            {
                foreach (var attribute in type.Attributes)
                {
                    if (attribute.Type.Is(iAspect!))
                    {
                        if (!aspectTypes.TryGetValue(attribute.Type, out var aspectType))
                        {
                            // TODO: handle AspectParts properly
                            aspectType = new AspectType(attribute.Type.FullName, aspectDriverFactory.GetAspectDriver(attribute.Type), new[] { new AspectPart(null, 0) });
                            aspectTypes.Add(attribute.Type, aspectType);
                        }

                        // TODO: create the aspect
                        results.Add(new AspectInstance(null!, type, aspectType));
                    }
                }
            }

            return results.ToImmutable();
        }
    }
}
