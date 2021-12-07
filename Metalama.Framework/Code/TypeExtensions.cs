// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code.Types;
using System;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Provides extension methods for <see cref="IType"/>.
    /// </summary>
    [CompileTimeOnly]
    public static class TypeExtensions
    {
        /// <summary>
        /// Creates an array type from the current type.
        /// </summary>
        /// <param name="elementType">Type of array elements.</param>
        /// <param name="rank">Rank of the array/.</param>
        /// <returns>An array type <c>T[]</c> where <c>T</c> is the current type.</returns>
        public static IArrayType ConstructArrayType( this IType elementType, int rank = 1 )
            => elementType.Compilation.TypeFactory.ConstructArrayType( elementType, rank );

        /// <summary>
        /// Creates an array type from the current type.
        /// </summary>
        /// <returns>An unsafe pointer type <c>*T</c> where <c>T</c> is the current type.</returns>
        public static IPointerType ConstructPointerType( this IType pointedType ) => pointedType.Compilation.TypeFactory.ConstructPointerType( pointedType );

        public static T ConstructNullable<T>( this T type )
            where T : IType
            => type.Compilation.TypeFactory.ConstructNullable( type );

        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Gets a value indicating whether the current type is assignable to another given type,
        /// given as an <see cref="IType"/>.
        /// </summary>
        /// <returns></returns>
        public static bool Is( this IType left, IType right, ConversionKind kind = default ) => left.Compilation.InvariantComparer.Is( left, right, kind );

        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Gets a value indicating whether the current type is assignable to another given type,
        /// given as a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right">Another type.</param>
        /// <returns></returns>
        public static bool Is( this IType left, Type right, ConversionKind kind = default ) => left.Compilation.InvariantComparer.Is( left, right, kind );

        public static bool Is( this IType left, SpecialType right, ConversionKind kind = default )
            => kind switch
            {
                ConversionKind.Implicit => left.SpecialType == right,
                ConversionKind.ImplicitReference => left.Is( left.Compilation.TypeFactory.GetSpecialType( right ), kind ),
                _ => throw new ArgumentOutOfRangeException( nameof(kind) )
            };

        /// <summary>
        /// Determines whether a type equals one of the well-known special types.
        /// </summary>
        public static bool Equals( this IType left, SpecialType right ) => left.SpecialType == right;

        /// <summary>
        /// Generates the <c>default(T)</c> syntax for the type.
        /// </summary>
        public static dynamic? DefaultValue( this IType type ) => type.Compilation.TypeFactory.DefaultValue( type );

        public static AsyncInfo GetAsyncInfo( this IType type ) => ((ICompilationInternal) type.Compilation).Helpers.GetAsyncInfo( type );
    }
}