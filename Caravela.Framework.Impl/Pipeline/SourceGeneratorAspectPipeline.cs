using Caravela.Framework.Impl.AspectOrdering;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Caravela.Framework.Impl.CompileTime;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Pipeline
{

    /// <summary>
    /// The main entry point of Caravela when called from a Roslyn source generator.
    /// </summary>
    internal class SourceGeneratorAspectPipeline : AspectPipeline
    {
        public SourceGeneratorAspectPipeline( IAspectPipelineContext context ) : base( context )
        {
        }

        public bool TryExecute( [NotNullWhen( true )] out IImmutableDictionary<string, SyntaxTree>? additionalSyntaxTrees )
        {

            if ( !this.TryExecuteCore( out var result ) )
            {
                additionalSyntaxTrees = null;
                return false;
            }

            additionalSyntaxTrees = result.AdditionalSyntaxTrees;
            return true;
        }

        public override bool CanTransformCompilation => false;

        /// <inheritdoc/>
        protected override HighLevelPipelineStage CreateStage( IReadOnlyList<OrderedAspectLayer> parts, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
            => new SourceGeneratorPipelineStage( parts, compileTimeAssemblyLoader, this );
    }
}
