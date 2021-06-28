// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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

        // ReSharper disable once UnusedTypeParameter

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