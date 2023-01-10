// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
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
        /// Determines if a given declaration is a child of another given declaration, using the <see cref="IDeclaration.ContainingDeclaration"/>
        /// relationship for all declarations except for named type, where the parent namespace is considered.
        /// </summary>
        public static bool IsContainedIn( this IDeclaration declaration, IDeclaration containingDeclaration )
        {
            var comparer = declaration.Compilation.Comparers.Default;

            if ( comparer.Equals( declaration.GetOriginalDefinition(), containingDeclaration.GetOriginalDefinition() ) )
            {
                return true;
            }

            if ( declaration is INamedType { ContainingDeclaration: not INamedType } namedType && containingDeclaration is INamespace containingNamespace )
            {
                return namedType.Namespace.IsContainedIn( containingNamespace );
            }

            return declaration.ContainingDeclaration != null && declaration.ContainingDeclaration.IsContainedIn( containingDeclaration );
        }

        /// <summary>
        /// Gets all containing ancestors, i.e. <c>declaration.ContainingDeclaration</c>, <c>declaration.ContainingDeclaration.ContainingDeclaration</c>,
        /// <c>declaration.ContainingDeclaration.ContainingDeclaration.ContainingDeclaration</c>... 
        /// </summary>
        public static IEnumerable<IDeclaration> ContainingAncestors( this IDeclaration declaration )
        {
            for ( var cursor = declaration.ContainingDeclaration; cursor != null; declaration = declaration.ContainingDeclaration )
            {
                yield return cursor;
            }
        }
        
        /// <summary>
        /// Gets all containing ancestors including the current declaration, i.e. <c>declaration</c>, <c>declaration.ContainingDeclaration</c>, <c>declaration.ContainingDeclaration.ContainingDeclaration</c>,
        /// <c>declaration.ContainingDeclaration.ContainingDeclaration.ContainingDeclaration</c>... 
        public static IEnumerable<IDeclaration> ContainedAncestorsAndSelf( this IDeclaration declaration )
        {
            for ( var cursor = declaration.ContainingDeclaration; cursor != null; declaration = declaration.ContainingDeclaration )
            {
                yield return cursor;
            }
        }
        
  
        /// <summary>
        /// Gets an object that gives access to the aspects on the current declaration.
        /// </summary>
        public static DeclarationEnhancements<T> Enhancements<T>( this T declaration )
            where T : class, IDeclaration
            => new( declaration );

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