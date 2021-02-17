using System.Collections.Generic;
using Caravela.Framework.Impl.CodeModel.Symbolic;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A <see cref="PipelineStage"/> that groups all aspects written with the high-level API instead of
    /// the <see cref="IAspectWeaver"/>.
    /// </summary>
    internal abstract class HighLevelAspectsPipelineStage : PipelineStage
    {
        private readonly CompileTimeAssemblyLoader _assemblyLoader;
        private readonly IReadOnlyList<AspectPart> _aspectParts;

        protected HighLevelAspectsPipelineStage( IReadOnlyList<AspectPart> aspectParts, CompileTimeAssemblyLoader assemblyLoader, IAspectPipelineProperties properties ) : base( properties )
        {
            this._aspectParts = aspectParts;
            this._assemblyLoader = assemblyLoader;
        }

        /// <inheritdoc/>
        public override PipelineStageResult ToResult( PipelineStageResult input )
        {
            var aspectPartResult = new AspectPartResult( new CompilationModel( input.Compilation ), this._assemblyLoader );

            foreach ( var aspectPart in this._aspectParts )
            {
                aspectPartResult = aspectPart.ToResult( aspectPartResult );
            }

            return this.GenerateCode( input, aspectPartResult );
        }

        /// <summary>
        /// Generates the code required by the aspects of a given <see cref="AspectPartResult"/>, and combine it with an input
        /// <see cref="PipelineStageResult"/> to produce an output <see cref="PipelineStageResult"/>.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="aspectPartResult"></param>
        /// <returns></returns>
        protected abstract PipelineStageResult GenerateCode( PipelineStageResult input, AspectPartResult aspectPartResult );
    }
}
