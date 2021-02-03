using Caravela.Framework.Impl.CompileTime;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl
{
    sealed class AdviceWeaverStage : PipelineStage
    {
        private readonly CompileTimeAssemblyLoader _assemblyLoader;
        private readonly List<AspectPart> _aspectParts;

        public IReadOnlyList<AspectPart> AspectParts => this._aspectParts;

        public AdviceWeaverStage( IEnumerable<AspectPart> aspectParts, CompileTimeAssemblyLoader assemblyLoader)
        {
            this._aspectParts = aspectParts.ToList();
            this._assemblyLoader = assemblyLoader;
        }

        public override PipelineStageResult ToResult( PipelineStageResult input )
        {
            var aspectPartResult = new AspectPartResult( input.Compilation, this._assemblyLoader );

            
            foreach ( var aspectPart in this._aspectParts )
            {
                aspectPartResult = aspectPart.ToResult( aspectPartResult );
            }

            // TODO: Aspect Linker goes here.

            return new PipelineStageResult(
                aspectPartResult.Compilation,
                aspectPartResult.Diagnostics,
                aspectPartResult.Resources,
                aspectPartResult.Aspects
                );
        }
    }
}
