// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Provides extension methods to work with generic declarations.
    /// </summary>
    [CompileTime]
    public static class GenericExtensions
    {
        /// <summary>
        /// Returns <c>true</c> if the current declaration, or any the declaring type, is generic.
        /// </summary>
        public static bool IsSelfOrAncestorGeneric( this IMemberOrNamedType declaration )
            => declaration is IGeneric { IsGeneric: true } || (declaration.DeclaringType != null && declaration.DeclaringType.IsSelfOrAncestorGeneric());

        /// <summary>
        /// Gets the original declaration of the <see cref="IDeclaration"/>, i.e. the declaration where all type parameters
        /// are unbound, including the ones of the containing types.
        /// </summary>
        public static IDeclaration GetOriginalDefinition( this IDeclaration declaration ) => GetOriginalDefinitionImpl( declaration );

        /// <summary>
        /// Gets the original declaration of the <see cref="IDeclaration"/>, i.e. the declaration where all type parameters
        /// are unbound, including the ones of the containing types.
        /// </summary>
        public static INamedType GetOriginalDefinition( this INamedType declaration ) => GetOriginalDefinitionImpl( declaration );

        /// <summary>
        /// Gets the original declaration of the <see cref="IDeclaration"/>, i.e. the declaration where all type parameters
        /// are unbound, including the ones of the containing types.
        /// </summary>
        public static IMemberOrNamedType GetOriginalDefinition( this IMemberOrNamedType declaration ) => GetOriginalDefinitionImpl( declaration );

        /// <summary>
        /// Gets the original declaration of the <see cref="IDeclaration"/>, i.e. the declaration where all type parameters
        /// are unbound, including the ones of the containing types.
        /// </summary>
        public static IMember GetOriginalDefinition( this IMember declaration ) => GetOriginalDefinitionImpl( declaration );

        /// <summary>
        /// Gets the original declaration of the <see cref="IDeclaration"/>, i.e. the declaration where all type parameters
        /// are unbound, including the ones of the containing types.
        /// </summary>
        public static IMethod GetOriginalDefinition( this IMethod declaration ) => GetOriginalDefinitionImpl( declaration );

        /// <summary>
        /// Gets the original declaration of the <see cref="IDeclaration"/>, i.e. the declaration where all type parameters
        /// are unbound, including the ones of the containing types.
        /// </summary>
        public static IProperty GetOriginalDefinition( this IProperty declaration ) => GetOriginalDefinitionImpl( declaration );

        /// <summary>
        /// Gets the original declaration of the <see cref="IDeclaration"/>, i.e. the declaration where all type parameters
        /// are unbound, including the ones of the containing types.
        /// </summary>
        public static IEvent GetOriginalDefinition( this IEvent declaration ) => GetOriginalDefinitionImpl( declaration );

        /// <summary>
        /// Gets the original declaration of the <see cref="IDeclaration"/>, i.e. the declaration where all type parameters
        /// are unbound, including the ones of the containing types.
        /// </summary>
        public static IConstructor GetOriginalDefinition( this IConstructor declaration ) => GetOriginalDefinitionImpl( declaration );

        /// <summary>
        /// Constructs a generic instance of an <see cref="INamedType"/>, with type arguments given as <see cref="IType"/>.
        /// </summary>
        public static INamedType ConstructGenericInstance( this INamedType declaration, params IType[] typeArguments )
            => (INamedType) ConstructGenericInstanceImpl( declaration, typeArguments );

        /// <summary>
        /// Constructs a generic instance of an <see cref="IMethod"/>, with type arguments given as <see cref="IType"/>.
        /// </summary>
        public static IMethod ConstructGenericInstance( this IMethod declaration, params IType[] typeArguments )
            => (IMethod) ConstructGenericInstanceImpl( declaration, typeArguments );

        /// <summary>
        /// Constructs a generic instance of an <see cref="INamedType"/>, with type arguments given a reflection <see cref="Type"/>.
        /// </summary>
        public static INamedType ConstructGenericInstance( this INamedType declaration, params Type[] typeArguments )
            => (INamedType) ConstructGenericInstanceImpl( declaration, typeArguments );

        /// <summary>
        /// Constructs a generic instance of an <see cref="IMethod"/>, with type arguments given as reflection <see cref="Type"/>.
        /// </summary>
        public static IMethod ConstructGenericInstance( this IMethod declaration, params Type[] typeArguments )
            => (IMethod) ConstructGenericInstanceImpl( declaration, typeArguments );

        /// <summary>
        /// Returns a representation of the current nested <see cref="INamedType"/>, but for a different generic instance
        /// of the declaring type.
        /// </summary>
        public static INamedType ForTypeInstance( this INamedType declaration, INamedType typeInstance )
            => (INamedType) ForTypeInstanceImpl( declaration, typeInstance );

        /// <summary>
        /// Returns a representation of the current <see cref="IField"/>, but for a different generic instance
        /// of the declaring type.
        /// </summary>
        public static IField ForTypeInstance( this IField declaration, INamedType typeInstance ) => (IField) ForTypeInstanceImpl( declaration, typeInstance );

        /// <summary>
        /// Returns a representation of the current <see cref="IMethod"/>, but for a different generic instance
        /// of the declaring type.
        /// </summary>
        public static IMethod ForTypeInstance( this IMethod declaration, INamedType typeInstance )
            => (IMethod) ForTypeInstanceImpl( declaration, typeInstance );

        /// <summary>
        /// Returns a representation of the current <see cref="IProperty"/>, but for a different generic instance
        /// of the declaring type.
        /// </summary>
        public static IProperty ForTypeInstance( this IProperty declaration, INamedType typeInstance )
            => (IProperty) ForTypeInstanceImpl( declaration, typeInstance );

        /// <summary>
        /// Returns a representation of the current <see cref="IEvent"/>, but for a different generic instance
        /// of the declaring type.
        /// </summary>
        public static IEvent ForTypeInstance( this IEvent declaration, INamedType typeInstance ) => (IEvent) ForTypeInstanceImpl( declaration, typeInstance );

        /// <summary>
        /// Returns a representation of the current <see cref="IConstructor"/>, but for a different generic instance
        /// of the declaring type.
        /// </summary>
        public static IConstructor ForTypeInstance( this IConstructor declaration, INamedType typeInstance )
            => (IConstructor) ForTypeInstanceImpl( declaration, typeInstance );

        private static T GetOriginalDefinitionImpl<T>( this T declaration )
            where T : class, IDeclaration
            => (T) ((IDeclarationInternal) declaration).OriginalDefinition;

        private static IGeneric ConstructGenericInstanceImpl( this IGeneric declaration, params IType[] typeArguments )
            => ((IGenericInternal) declaration).ConstructGenericInstance( typeArguments );

        private static IGeneric ConstructGenericInstanceImpl( this IGeneric declaration, params Type[] typeArguments )
            => ((IGenericInternal) declaration).ConstructGenericInstance( typeArguments.Select( TypeFactory.GetType ).ToArray() );

        private static IMemberOrNamedType ForTypeInstanceImpl( this IMemberOrNamedType declaration, INamedType typeInstance )
        {
            if ( declaration.DeclaringType == null )
            {
                throw new InvalidOperationException( $"The type '{declaration.ToDisplayString()}' is not a nested type." );
            }

            if ( typeInstance.IsOpenGeneric )
            {
                throw new ArgumentOutOfRangeException( nameof(typeInstance), $"The type '{typeInstance.ToDisplayString()}' has unbound generic parameters." );
            }

            var thisOriginalDeclaration = declaration.GetOriginalDefinition();

            if ( !declaration.Compilation.InvariantComparer.Equals( typeInstance.GetOriginalDefinition(), thisOriginalDeclaration.DeclaringType! ) )
            {
                throw new ArgumentOutOfRangeException(
                    nameof(typeInstance),
                    $"The type must be identical to or constructed from '{thisOriginalDeclaration.DeclaringType!.ToDisplayString()}'." );
            }

            if ( !declaration.DeclaringType.IsSelfOrAncestorGeneric() )
            {
                return declaration;
            }

            IEnumerable<IMemberOrNamedType> candidates;

            switch ( declaration )
            {
                case INamedType namedType:
                    candidates = typeInstance.NestedTypes.OfName( namedType.Name );

                    break;

                case IMethod method:
                    candidates = typeInstance.Methods.OfName( method.Name );

                    break;

                case IField field:
                    candidates = typeInstance.Fields.OfName( field.Name );

                    break;

                case IProperty property:
                    candidates = typeInstance.Properties.OfName( property.Name );

                    break;

                case IEvent @event:
                    candidates = typeInstance.Events.OfName( @event.Name );

                    break;

                case IConstructor { IsStatic: false }:
                    candidates = typeInstance.Constructors;

                    break;

                case IConstructor { IsStatic: true }:
                    candidates = typeInstance.StaticConstructor != null ? new[] { typeInstance.StaticConstructor } : Array.Empty<IMemberOrNamedType>();

                    break;

                default:
                    throw new ArgumentOutOfRangeException( nameof(declaration) );
            }

            return candidates.Single( c => declaration.Compilation.InvariantComparer.Equals( c.GetOriginalDefinition(), thisOriginalDeclaration ) );
        }
    }
}