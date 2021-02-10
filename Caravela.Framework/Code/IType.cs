using System;
using Caravela.Framework.Project;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a constructed type, for instance an array, a generic type instance, a pointer.
    /// A class, struct, enum or delegate are represented as an <see cref="INamedType"/>, which
    /// derive from <see cref="IType"/>.
    /// </summary>
    [CompileTime]
    public interface IType : IDisplayable
    {
        /// <summary>
        /// Gets the kind of type.
        /// </summary>
        TypeKind TypeKind { get; }

        ITypeFactory TypeFactory { get; }

        // TODO: Define Is(string)
        // TODO: Define Equals

    }

    public static class TypeExtensions
    {

        /// <summary>
        /// Creates an array type from the current type.
        /// </summary>
        /// <param name="rank">Rank of the array/.</param>
        /// <returns>An array type <c>T[]</c> where <c>T</c> is the current type.</returns>
        public static IArrayType MakeArrayType( this IType elementType, int rank = 1 ) =>
            elementType.TypeFactory.MakeArrayType( elementType, rank );

        /// <summary>
        /// Creates an array type from the current type.
        /// </summary>
        /// <returns>An unsafe pointer type <c>*T</c> where <c>T</c> is the current type.</returns>
        public static IPointerType MakePointerType( this IType pointedType ) =>
            pointedType.TypeFactory.MakePointerType( pointedType );

        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Determines whether the current type is assignable to another given type,
        /// given as an <see cref="IType"/>.
        /// </summary>
        /// <param name="other">Another type.</param>
        /// <returns></returns>
        public static bool Is( this IType left, IType right ) =>
            left.TypeFactory.Is( left, right );

        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Determines whether the current type is assignable to another given type,
        /// given as a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="right">Another type.</param>
        /// <returns></returns>
        public static bool Is( this IType left, Type right ) =>
            left.TypeFactory.Is( left, right );
    }
    

    public interface ITypeFactory
    {
        /// <summary>
        /// Get type based on its full name, as used in reflection.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For nested types, this means using <c>+</c>, e.g. to get <see cref="System.Environment.SpecialFolder"/>, use <c>System.Environment+SpecialFolder</c>.
        /// </para>
        /// <para>
        /// For generic type definitions, this requires using <c>`</c>, e.g. to get <see cref="List{T}"/>, use <c>System.Collections.Generic.List`1</c>.
        /// </para>
        /// <para>
        /// Constructed generic types (e.g. <c>List&lt;int&gt;</c>) are not supported, for those, use <see cref="INamedType.MakeGenericType"/>.
        /// </para>
        /// </remarks>
        INamedType? GetTypeByReflectionName( string reflectionName );

        IType? GetTypeByReflectionType( Type type );
        
        IArrayType MakeArrayType( IType elementType, int rank );
        IPointerType MakePointerType( IType pointedType );
        
        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Determines whether the current type is assignable to another given type,
        /// given as an <see cref="IType"/>.
        /// </summary>
        /// <param name="other">Another type.</param>
        /// <returns></returns>
        bool Is( IType left, IType right );

        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Determines whether the current type is assignable to another given type,
        /// given as a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="other">Another type.</param>
        /// <returns></returns>
        bool Is( IType left, Type right );
    }
}