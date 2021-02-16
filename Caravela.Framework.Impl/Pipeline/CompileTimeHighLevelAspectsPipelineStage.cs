using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.CodeModel.Builders;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Linking;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The implementation of <see cref="HighLevelAspectsPipelineStage"/> used at compile time (not at design time).
    /// </summary>
    internal class CompileTimeHighLevelAspectsPipelineStage : HighLevelAspectsPipelineStage
    {
        public CompileTimeHighLevelAspectsPipelineStage( IReadOnlyList<AspectPart> aspectParts, CompileTimeAssemblyLoader assemblyLoader, IAspectPipelineOptions options ) : base( aspectParts, assemblyLoader, options )
        {
        }

        /// <inheritdoc/>
        protected override PipelineStageResult GenerateCode( PipelineStageResult input, AspectPartResult aspectPartResult )
        {
            var linker = new AspectLinker( new AdviceLinkerInput( input.Compilation, aspectPartResult.Compilation, aspectPartResult.Transformations, input.AspectParts ) );
            var linkerResult = linker.ToResult();

            return new PipelineStageResult(
                linkerResult.Compilation,
                input.AspectParts,
                aspectPartResult.Diagnostics.Concat( linkerResult.Diagnostics ).ToList(),
                aspectPartResult.Transformations.OfType<ManagedResourceBuilder>().Select( r => r.ToResourceDescription() ).ToList(),
                aspectPartResult.AspectSources );
        }
    }
}