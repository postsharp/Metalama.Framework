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
        private SourceGeneratorAspectPipeline( IAspectPipelineContext context ) : base( context, new Options() )
        {
        }

        public static bool TryExecute( IAspectPipelineContext context, [NotNullWhen( true )] out IImmutableDictionary<string, SyntaxTree>? additionalSyntaxTrees )
        {
            var pipeline = new SourceGeneratorAspectPipeline( context );

            if ( !pipeline.TryExecute( out var result ) )
            {
                additionalSyntaxTrees = null;
                return false;
            }

            additionalSyntaxTrees = result.AdditionalSyntaxTrees;
            return true;
        }

        private class Options : IAspectPipelineOptions
        {
            public bool CanTransformCompilation => false;
        }

        /// <inheritdoc/>
        protected override HighLevelAspectsPipelineStage CreateStage( IReadOnlyList<AspectPart> parts, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
            => new SourceGeneratorHighLevelAspectsPipelineStage( parts, compileTimeAssemblyLoader, this.PipelineOptions );
    }
}
