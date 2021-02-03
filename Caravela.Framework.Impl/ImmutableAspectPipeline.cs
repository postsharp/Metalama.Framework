using Caravela.Compiler;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl
{
    [Transformer]
    class ImmutableAspectPipeline : AspectPipeline, ISourceTransformer
    {
    }
}
