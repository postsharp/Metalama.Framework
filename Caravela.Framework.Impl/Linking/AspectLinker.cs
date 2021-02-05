using Caravela.Framework.Impl.CodeModel;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Linking
{
    class AspectLinker
    {
        public AdviceLinkerResult ToResult(AdviceLinkerContext context)
        {
            var compilationToBeLinked = new ModifiedCompilationModel( context.Compilation, context.Transformations );



            return new AdviceLinkerResult( , Array.Empty<Diagnostic>().ToImmutableReactive() );
        }
    }
}
