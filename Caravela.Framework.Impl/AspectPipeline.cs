﻿using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Caravela.Framework.Sdk;
using RoslynEx;

namespace Caravela.Framework.Impl
{
    [Transformer]
    sealed class AspectPipeline : ISourceTransformer
    {
        public Microsoft.CodeAnalysis.Compilation Execute(TransformerContext context)
        {
            var compilation = (CSharpCompilation)context.Compilation;

            // DI
            var loader = new Loader(context.LoadReferencedAssembly);
            var driverFactory = new AspectDriverFactory(compilation, loader);
            var aspectTypeFactory = new AspectTypeFactory(driverFactory);

            // TODO: how to get other sources?
            var aspectSources = new AspectSource[] { new AttributeAspectSource(compilation, aspectTypeFactory) };

            var aspectParts = from source in aspectSources
                              from aspect in source.GetAspects()
                              group aspect by aspect.AspectType into g
                              let aspectType = g.Key
                              from aspectPart in aspectType.Parts
                              orderby aspectPart.ExecutionOrder
                              select (aspects: g.AsEnumerable(), aspectType, aspectPart);

            // TODO: aspect part grouping

            var aspectCompilation = new AspectCompilation(ImmutableArray.Create<Diagnostic>(), compilation);

            foreach (var aspectPart in aspectParts)
            {
                PipelineStage stage = aspectPart.aspectType.AspectDriver switch
                {
                    IAspectWeaver weaver => new AspectWeaverStage(
                        weaver, aspectCompilation.Compilation.GetTypeByMetadataName(aspectPart.aspectType.Name)!, aspectPart.aspects.ToImmutableArray())
                };

                aspectCompilation = stage.Transform(aspectCompilation);
            }

            return aspectCompilation.Compilation;
        }
    }
}
