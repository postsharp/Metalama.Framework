using System.Collections.Generic;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;

namespace Caravela.Framework.Impl
{
    internal abstract class AdviceWeaverStage : PipelineStage
    {
        private readonly CompileTimeAssemblyLoader _assemblyLoader;
        private readonly IReadOnlyList<AspectPart> _aspectParts;

        public AdviceWeaverStage( IReadOnlyList<AspectPart> aspectParts, CompileTimeAssemblyLoader assemblyLoader )
        {
            this._aspectParts = aspectParts;
            this._assemblyLoader = assemblyLoader;
        }

        public override PipelineStageResult ToResult( PipelineStageResult input )
        {
            var aspectPartResult = new AspectPartResult( new CompilationModel( input.Compilation ), this._assemblyLoader );

            foreach ( var aspectPart in this._aspectParts )
            {
                aspectPartResult = aspectPart.ToResult( aspectPartResult );
            }

            return GenerateCode(input, aspectPartResult);
        }

        protected abstract PipelineStageResult GenerateCode( PipelineStageResult input, AspectPartResult aspectPartResult );
    }
}
