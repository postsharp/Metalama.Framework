using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using Caravela.Reactive;

namespace Caravela.Framework.Impl
{
    internal sealed class AdviceWeaverStage : PipelineStage
    {
        private readonly CompileTimeAssemblyLoader _assemblyLoader;
        private readonly List<AspectPart> _aspectParts;

        public IReadOnlyList<AspectPart> AspectParts => this._aspectParts;

        public AdviceWeaverStage( IEnumerable<AspectPart> aspectParts, CompileTimeAssemblyLoader assemblyLoader )
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
            var linker = new AspectLinker();
            var linkerContext = new AdviceLinkerContext( aspectPartResult.Compilation, aspectPartResult.Transformations.Where( x => x is OverriddenElement ).ToImmutableReactive() );
            var linkerResult = linker.ToResult( linkerContext );

            return new PipelineStageResult(
                linkerResult.Compilation,
                aspectPartResult.Diagnostics.Concat( linkerResult.Diagnostics.GetValue() ).ToList(),
                aspectPartResult.Resources,
                aspectPartResult.Aspects );
        }
    }
}
