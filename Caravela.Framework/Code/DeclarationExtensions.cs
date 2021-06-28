using Caravela.Framework.Aspects;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Code
{
    [CompileTimeOnly]
    public static class DeclarationExtensions
    {
        public static IEnumerable<T> Aspects<T>( this IDeclaration declaration )
            where T : IAspect
            => declaration.Compilation.GetAspectsOf<T>( declaration );

        /// <summary>
        /// Gets the list of annotations registered on the current declaration for a given aspect type.
        /// </summary>
        /// <typeparam name="T">The type of the aspect for which the annotations are requested.</typeparam>
        [Obsolete( "Not implemented." )]
        public static IAnnotationList Annotations<T>( this IDeclaration declaration )
            where T : IAspect
        {
            throw new NotImplementedException();
        }
    }
}