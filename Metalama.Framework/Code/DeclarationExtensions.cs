// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Extension methods for <see cref="IDeclaration"/>.
    /// </summary>
    [CompileTime]
    [PublicAPI]
    public static class DeclarationExtensions
    {
        /// <summary>
        /// Determines if a given declaration is a child of another given declaration, using the <see cref="IDeclaration.ContainingDeclaration"/>
        /// relationship for all declarations except for named type, where the parent namespace is considered.
        /// </summary>
        public static bool IsContainedIn( this IDeclaration declaration, IDeclaration containingDeclaration )
        {
            var comparer = declaration.Compilation.Comparers.Default;

            if ( comparer.Equals( declaration.GetDefinition(), containingDeclaration.GetDefinition() ) )
            {
                return true;
            }

            if ( declaration is INamedType { ContainingDeclaration: not INamedType } namedType && containingDeclaration is INamespace containingNamespace )
            {
                return namedType.ContainingNamespace.IsContainedIn( containingNamespace );
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
        /// Gets an object that gives access to the aspects, options and annotations on the current declaration.
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
                // ToNonNullableType() can either return an INamedType or an ITypeParameter. In the second case, we don't have a meaningful "closest named type".
                INamedType namedType => namedType.ToNonNullableType() as INamedType,
                IMember member => member.DeclaringType.ToNonNullableType() as INamedType,
                { ContainingDeclaration: { } containingDeclaration } => GetClosestNamedType( containingDeclaration ),
                _ => null
            };

        /// <summary>
        /// Gets the declaring <see cref="IMemberOrNamedType"/> of a given declaration if the declaration if not an <see cref="IMemberOrNamedType"/>, or the <see cref="IMemberOrNamedType"/> itself if the given declaration is itself an <see cref="IMemberOrNamedType"/>. 
        /// </summary>
        public static IMemberOrNamedType? GetClosestMemberOrNamedType( this IDeclaration declaration )
            => declaration switch
            {
                // ToNonNullableType() can either return an INamedType or an ITypeParameter. In the second case, we don't have a meaningful "closest named type".
                INamedType namedType => namedType.ToNonNullableType() as INamedType,
                IMember member => member,
                { ContainingDeclaration: { } containingDeclaration } => GetClosestMemberOrNamedType( containingDeclaration ),
                _ => null
            };

        /// <summary>
        /// Gets the topmost type of a nested type, i.e. a type that is not contained in any other type. If the given type is not a given type,
        /// returns the given type itself. 
        /// </summary>
        public static INamedType? GetTopmostNamedType( this IDeclaration declaration )
            => declaration switch
            {
                // ToNonNullableType() can either return an INamedType or an ITypeParameter. In the second case, we don't have a meaningful "closest named type".
                INamedType { DeclaringType: null } namedType => namedType.ToNonNullableType() as INamedType,
                INamedType { DeclaringType: not null } namedType => namedType.DeclaringType.GetTopmostNamedType(),
                _ => declaration.GetClosestNamedType()?.GetTopmostNamedType()
            };

        /// <summary>
        /// Gets the namespace of a given declaration, i.e. the namespace itself if the given declaration is a namespace,
        /// the closest containing namespace, or the global namespace if an <see cref="ICompilation"/> is given.
        /// </summary>
        public static INamespace? GetNamespace( this IDeclaration declaration )
            => declaration switch
            {
                INamespace ns => ns,
                ICompilation compilation => compilation.GlobalNamespace,
                _ => declaration.GetTopmostNamedType()?.ContainingNamespace
            };

        /// <summary>
        /// Gets a representation of the current declaration in a different version of the compilation.
        /// </summary>
        [return: NotNullIfNotNull( nameof(compilationElement) )]
        public static T? ForCompilation<T>( this T? compilationElement, ICompilation compilation, ReferenceResolutionOptions options = default )
            where T : class, ICompilationElement
        {
            if ( compilationElement == null )
            {
                return null;
            }
            else
            {
                return
                    ((ICompilationInternal) compilation).Factory.Translate( compilationElement, options )
                    ?? throw new InvalidOperationException(
                        $"The declaration '{compilationElement}' does not exist in the requested compilation. "
                        + $"Use TryForCompilation to avoid this exception." );
            }
        }

        /// <summary>
        /// Tries to get a representation of the current declaration in a different version of the compilation.
        /// </summary>
        public static bool TryForCompilation<T>(
            this T? compilationElement,
            ICompilation compilation,
            [NotNullWhen( true )] out T? translated,
            ReferenceResolutionOptions options = default )
            where T : class, ICompilationElement
        {
            if ( compilationElement == null )
            {
                translated = null;

                return false;
            }
            else
            {
                translated = ((ICompilationInternal) compilation).Factory.Translate( compilationElement, options );

                return translated != null;
            }
        }

        public static bool IsRecordCopyConstructor( this IConstructor constructor )
            => constructor is
            {
                IsStatic: false,
                IsImplicitlyDeclared: true,
                IsPrimary: false,
                Parameters: [_],
                DeclaringType.TypeKind: TypeKind.RecordClass or TypeKind.RecordStruct
            };

        /// <summary>
        /// Gets the declarations (namespaces, types, methods, properties, fields, constructors, events, indexers) directly contained in the given declaration.
        /// </summary>
        /// <remarks>The method does not descent into accessors, custom attributes, parameters, or type parameters.</remarks>
        public static IEnumerable<IDeclaration> ContainedChildren( this IDeclaration declaration )
            => declaration switch
            {
                ICompilation compilation => compilation.Types,
                INamespace ns => ns.Namespaces.Concat<IDeclaration>( ns.Types ),
                INamedType type => type.Members().Concat<IDeclaration>( type.Types ),
                _ => []
            };

        /// <summary>
        /// Gets the declarations (namespaces, types, methods, properties, fields, constructors, events, indexers) in the given declaration
        /// and in any child of the declaration.
        /// </summary>
        /// <remarks>The method does not descent into accessors, custom attributes, parameters, or type parameters.</remarks>
        public static IEnumerable<IDeclaration> ContainedDescendants( this IDeclaration declaration )
            => declaration.SelectManyRecursive( d => d.ContainedChildren() );

        /// <summary>
        /// Gets the declarations (namespaces, types, methods, properties, fields, constructors, events, indexers) in the given declaration
        /// and in any child of the declaration, plus the given declaration.
        /// </summary>
        /// <remarks>The method does not descent into accessors, custom attributes, parameters, or type parameters.</remarks>
        public static IEnumerable<IDeclaration> ContainedDescendantsAndSelf( this IDeclaration declaration )
            => declaration.SelectManyRecursive( d => d.ContainedChildren(), true );
    }
}