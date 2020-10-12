using System;
using System.Collections.Generic;
using System.Linq;
using PostSharp.Framework.Sdk;
using RoslynEx;

namespace PostSharp.Framework.Impl
{
    [Transformer]
    class AspectPipeline : ISourceTransformer
    {
        public Microsoft.CodeAnalysis.Compilation Execute(TransformerContext context)
        {
            // TODO?
            var aspectSources = new AspectSource[] { new AttributeAspectSource(context.Compilation) };
            var aspects = aspectSources.SelectMany(s => s.Aspects);

            var driverFactory = new AspectDriverFactory(context.Compilation);
            
            // need to have aspect parts to order even for weaver, maybe?
        }
    }

    abstract class AspectSource
    {
        public abstract IReadOnlyList<AspectInstance> Aspects { get; }
    }

    class AttributeAspectSource : AspectSource
    {
        private Microsoft.CodeAnalysis.Compilation compilation;

        public AttributeAspectSource(Microsoft.CodeAnalysis.Compilation compilation)
        {
            this.compilation = compilation;
        }

        public override IReadOnlyList<AspectInstance> Aspects => throw new NotImplementedException();
    }
}
