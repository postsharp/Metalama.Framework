// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Extension methods for <see cref="IDeclaration"/>.
    /// </summary>
    [CompileTime]
    public static class DeclarationExtensions
    {
        /// <summary>
        /// Gets the set of instances of a specified type of aspects that have been applied to a specified declaration.
        /// </summary>
        /// <param name="declaration">The declaration.</param>
        /// <typeparam name="T">The exact type of aspects.</typeparam>
        /// <returns>The set of aspects of exact type <typeparamref name="T"/> applied on <paramref name="declaration"/>.</returns>
        /// <remarks>
        /// You can call this method only for aspects that have been already been applied or are being applied, i.e. you can query aspects
        /// that are applied before the current aspect, or you can query instances of the current aspects applied in a parent class.
        /// </remarks>
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

        /// <summary>
        /// Gets the declaring <see cref="INamedType"/> of a given declaration if the declaration if not an <see cref="INamedType"/>, or the <see cref="INamedType"/> itself if the given declaration is itself an <see cref="INamedType"/>. 
        /// </summary>
        public static INamedType? GetNamedType( this IDeclaration declaration )
            => declaration switch
            {
                INamedType namedType => namedType,
                IMember member => member.DeclaringType,
                { ContainingDeclaration: { } containingDeclaration } => GetNamedType( containingDeclaration ),
                _ => null
            };

        public static INamedType? GetTopNamedType( this IDeclaration declaration )
            => declaration switch
            {
                INamedType { DeclaringType: null } namedType => namedType,
                INamedType { DeclaringType: { } } namedType => namedType.DeclaringType.GetTopNamedType(),
                _ => declaration.GetNamedType()?.GetTopNamedType()
            };

        /// <summary>
        /// Gets a representation of the current declaration in a different version of the compilation.
        /// </summary>
        [return: NotNullIfNotNull( "declaration" )]
        public static T? ForCompilation<T>( this T? declaration, ICompilation compilation, ReferenceResolutionOptions options = default )
            where T : class, IDeclaration
        {
            if ( declaration == null )
            {
                return null;
            }
            else
            {
                return (T) ((ICompilationInternal) compilation).Factory.Translate( declaration, options );
            }
        }
    }
}