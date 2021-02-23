using Caravela.Compiler;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The main compile-time entry point of Caravela. An implementation of Caravela.Compiler's <see cref="ISourceTransformer"/>.
    /// </summary>
    [Transformer]
    internal sealed partial class AspectPipelineTransformer : ISourceTransformer
    {
        public Compilation Execute( TransformerContext transformerContext )
        {
            using CompileTimeAspectPipeline pipeline = new( new AspectPipelineContext( transformerContext ) );
            if ( pipeline.TryExecute( out var compilation ) )
            {
                return compilation;
            }
            else
            {
                return transformerContext.Compilation;
            }
        }
    }
}
