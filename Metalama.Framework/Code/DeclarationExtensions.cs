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
            for ( var cursor = declaration.ContainingDeclaration; cursor != null; cursor = cursor.ContainingDeclaration )
            {
                yield return cursor;
            }
        }

        /// <summary>
        /// Gets all containing ancestors including the current declaration, i.e. <c>declaration</c>, <c>declaration.ContainingDeclaration</c>, <c>declaration.ContainingDeclaration.ContainingDeclaration</c>,
        /// <c>declaration.ContainingDeclaration.ContainingDeclaration.ContainingDeclaration</c>... 
        /// </summary>
        public static IEnumerable<IDeclaration> ContainingAncestorsAndSelf( this IDeclaration declaration )
        {
            for ( var cursor = declaration; cursor != null; cursor = cursor.ContainingDeclaration )
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
                INamedType namedType => namedType.ToNonNullableType(),
                IMember member => member.DeclaringType.ToNonNullableType(),
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
                INamedType { DeclaringType: null } namedType => namedType.ToNonNullableType(),
                INamedType { DeclaringType: not null } namedType => namedType.DeclaringType.GetTopmostNamedType(),
                _ => declaration.GetClosestNamedType()?.GetTopmostNamedType()
            };

        /// <summary>
        /// Gets a representation of the current declaration in a different version of the compilation.
        /// </summary>
        [return: NotNullIfNotNull( "compilationElement" )]
        public static T? ForCompilation<T>( this T? compilationElement, ICompilation compilation, ReferenceResolutionOptions options = default )
            where T : class, ICompilationElement
        {
            if ( compilationElement == null )
            {
                return null;
            }
            else
            {
                return (T) ((ICompilationInternal) compilation).Factory.Translate( compilationElement, options );
            }
        }

        public static IFieldOrProperty? ForCompilation(
            this IFieldOrProperty? fieldOrProperty,
            ICompilation compilation,
            ReferenceResolutionOptions options = default )
        {
            if ( fieldOrProperty == null )
            {
                return null;
            }
            else
            {
                return (IFieldOrProperty) ((ICompilationInternal) compilation).Factory.Translate( fieldOrProperty, options );
            }
        }

        public static IDeclaration? GetBaseDefinition( this IMemberOrNamedType declaration )
            => declaration.DeclarationKind switch
            {
                DeclarationKind.NamedType => ((INamedType) declaration).BaseType?.TypeDefinition,
                DeclarationKind.Method => ((IMethod) declaration).OverriddenMethod?.MethodDefinition,
                DeclarationKind.Property => ((IProperty) declaration).OverriddenProperty?.PropertyDefinition,
                DeclarationKind.Event => ((IEvent) declaration).OverriddenEvent?.EventDefinition,
                DeclarationKind.Indexer => ((IIndexer) declaration).OverriddenIndexer?.IndexerDefinition,
                _ => null
            };
    }
}