using Caravela.Framework.Impl.CompileTime;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{

    internal class SourceGeneratorAspectPipeline : AspectPipeline
    {
        private SourceGeneratorAspectPipeline( IAspectPipelineContext context ) : base( context, new Options() )
        {
        }

        public static bool TryExecute(IAspectPipelineContext context, [NotNullWhen(true)] out IImmutableDictionary<string, SyntaxTree>? additionalSyntaxTrees )
        {
            var pipeline = new SourceGeneratorAspectPipeline( context );

            if ( !pipeline.TryExecute( out var result ))
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

            public bool CanAddSyntaxTrees => true;
        }

        protected override AdviceWeaverStage CreateAdviceWeaverStage( IReadOnlyList<AspectPart> parts, CompileTimeAssemblyLoader compileTimeAssemblyLoader )
            => new SourceGeneratorAdviceWeaverStage( parts, compileTimeAssemblyLoader );
    }
}
