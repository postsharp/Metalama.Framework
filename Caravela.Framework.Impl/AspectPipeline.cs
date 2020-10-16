using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Sdk;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RoslynEx;

namespace Caravela.Framework.Impl
{
    [Transformer]
    sealed class AspectPipeline : ISourceTransformer
    {
        public Microsoft.CodeAnalysis.Compilation Execute(TransformerContext context)
        {
            var roslynCompilation = (CSharpCompilation)context.Compilation;

            // DI
            var loader = new Loader(context.LoadReferencedAssembly);
            var compilation = new Compilation(roslynCompilation);
            var driverFactory = new AspectDriverFactory(compilation, loader);
            var aspectTypeFactory = new AspectTypeFactory(driverFactory);

            var aspectCompilation = new AspectCompilation(ImmutableArray.Create<Diagnostic>(), compilation);

            // TODO: either change this to get all AspectParts from the initial compilation (not just those that have AspectInstances)
            // TODO: or change it so that AspectParts are updated after every stage

            var aspectParts = aspectCompilation.Aspects
                .GroupBy(a => a.AspectType)
                .Select((g, token) => (aspects: g.GetValue(token), aspectType: aspectTypeFactory.GetAspectType(g.Key, token)))
                .SelectMany(
                    x => x.aspectType.Parts,
                    (x, aspectPart) => (x.aspects, x.aspectType, aspectPart))
                .GetValue(default)
                .OrderBy(x => x.aspectPart.ExecutionOrder);

            // TODO: aspect part grouping

            foreach (var aspectPart in aspectParts)
            {
                PipelineStage stage = aspectPart.aspectType.AspectDriver switch
                {
                    IAspectWeaver weaver => new AspectWeaverStage(
                        weaver, ((Compilation)aspectCompilation.Compilation).RoslynCompilation.GetTypeByMetadataName(aspectPart.aspectType.Name)!,
                        aspectPart.aspects.ToImmutableArray())
                };

                aspectCompilation = stage.Transform(aspectCompilation);
            }

            foreach (var diagnostic in aspectCompilation.Diagnostics)
            {
                context.ReportDiagnostic(diagnostic);
            }

            return ((Compilation)aspectCompilation.Compilation).RoslynCompilation;
        }
    }
}
