// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Extension methods for <see cref="IDeclaration"/>.
    /// </summary>
    [CompileTime]
    public static class DeclarationExtensions
    {
        public static DeclarationEnhancements Enhancements( this IDeclaration declaration ) => new( declaration );

        /// <summary>
        /// Gets the declaring <see cref="INamedType"/> of a given declaration if the declaration if not an <see cref="INamedType"/>, or the <see cref="INamedType"/> itself if the given declaration is itself an <see cref="INamedType"/>. 
        /// </summary>
        public static INamedType? GetClosestNamedType( this IDeclaration declaration )
            => declaration switch
            {
                INamedType namedType => namedType,
                IMember member => member.DeclaringType,
                { ContainingDeclaration: { } containingDeclaration } => GetClosestNamedType( containingDeclaration ),
                _ => null
            };

        /// <summary>
        /// Gets the topmost type of a nested type, i.e. a type that is not contained in any other type. If the given type is not a given type,
        /// returns the given type itself. 
        /// </summary>
        public static INamedType? GetTopmostNamedType( this IDeclaration declaration )
            => declaration switch
            {
                INamedType { DeclaringType: null } namedType => namedType,
                INamedType { DeclaringType: { } } namedType => namedType.DeclaringType.GetTopmostNamedType(),
                _ => declaration.GetClosestNamedType()?.GetTopmostNamedType()
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