using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using Caravela.Reactive;
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
            AspectLinker linker = new AspectLinker();
            AdviceLinkerContext linkerContext = new AdviceLinkerContext( aspectPartResult.Compilation, aspectPartResult.Transformations.Where( x => x is OverriddenElement ).ToImmutableReactive() );
            AdviceLinkerResult linkerResult = linker.ToResult( linkerContext );

            return new PipelineStageResult(
                linkerResult.Compilation,
                aspectPartResult.Diagnostics.Concat( linkerResult.Diagnostics.GetValue() ).ToList(),
                aspectPartResult.Resources,
                aspectPartResult.Aspects
                );
        }
    }
}
