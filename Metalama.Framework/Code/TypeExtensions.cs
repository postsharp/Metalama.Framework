// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Provides extension methods for <see cref="IType"/>.
    /// </summary>
    [CompileTime]
    public static class TypeExtensions
    {
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
                ConversionKind.ImplicitReference => left.Is( ((ICompilationInternal) left.Compilation).Factory.GetSpecialType( right ), kind ),
                _ => throw new ArgumentOutOfRangeException( nameof(kind) )
            };

        public static bool Equals__( this IType left, SpecialType right ) => left.SpecialType == right;

        /// <summary>
        /// Generates the <c>default(T)</c> syntax for the type.
        /// </summary>
        public static dynamic? DefaultValue( this IType type ) => ((ICompilationInternal) type.Compilation).Factory.DefaultValue( type );

        public static AsyncInfo GetAsyncInfo( this IType type ) => ((ICompilationInternal) type.Compilation).Helpers.GetAsyncInfo( type );
    }
}