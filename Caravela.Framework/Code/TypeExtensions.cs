using System;

namespace Caravela.Framework.Code
{
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
}