using System;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
    internal class AspectLinker
    {
        public AdviceLinkerResult ToResult( AdviceLinkerContext context )
        {
            var compilationToBeLinked = new ModifiedCompilationModel( context.Compilation, context.Transformations );

            return new AdviceLinkerResult( compilationToBeLinked, Array.Empty<Diagnostic>().ToImmutableReactive() );
        }
    }
}
