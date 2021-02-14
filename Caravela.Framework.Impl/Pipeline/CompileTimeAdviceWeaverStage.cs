// unset

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl
{
    internal class CompileTimeAdviceWeaverStage : AdviceWeaverStage
    {
        public CompileTimeAdviceWeaverStage(IReadOnlyList<AspectPart> aspectParts, CompileTimeAssemblyLoader assemblyLoader) : base(aspectParts, assemblyLoader)
        {
        }
        
        protected override PipelineStageResult GenerateCode(PipelineStageResult input, AspectPartResult aspectPartResult)
        {
            var linker = new AspectLinker(new AdviceLinkerInput(input.Compilation, aspectPartResult.Compilation, aspectPartResult.Transformations, input.AspectParts));
            var linkerResult = linker.ToResult();

            return new PipelineStageResult(
                linkerResult.Compilation,
                input.AspectParts,
                aspectPartResult.Diagnostics.Concat(linkerResult.Diagnostics).ToList(),
                aspectPartResult.Transformations.OfType<ManagedResourceBuilder>().Select(r => r.ToResourceDescription()).ToList(),
                aspectPartResult.AspectSources);
        }
    }
}