// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code.Comparers;
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
        public static bool Is( this IType left, IType right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default )
            => left.Compilation.Comparers.GetTypeComparer( typeComparison ).Is( left, right, kind );

        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Gets a value indicating whether the current type is assignable to another given type,
        /// given as a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right">Another type.</param>
        public static bool Is( this IType left, Type right, ConversionKind kind = default, TypeComparison typeComparison = TypeComparison.Default )
            => left.Compilation.Comparers.GetTypeComparer( typeComparison ).Is( left, right, kind );

        /// <summary>
        /// Equivalent to the <c>is</c> operator in C#. Gets a value indicating whether the current type is assignable to another given type,
        /// given as a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right">Another type.</param>
        public static bool Is( this IType left, SpecialType right, ConversionKind kind = default )
            => kind switch
            {
                ConversionKind.Implicit => left.SpecialType == right,
                ConversionKind.ImplicitReference => left.Is( ((ICompilationInternal) left.Compilation).Factory.GetSpecialType( right ), kind ),
                _ => throw new ArgumentOutOfRangeException( nameof(kind) )
            };

        /// <summary>
        /// Determines if a type derives from another one, given as an <see cref="INamedType"/>.
        /// </summary>
        /// <param name="left">The child type.</param>
        /// <param name="right">The base type. It cannot be a generic type instance.</param>
        /// <param name="options">Determine with inheritance relationships should be considered.</param>
        public static bool DerivesFrom( this INamedType left, INamedType right, DerivedTypesOptions options = DerivedTypesOptions.Default )
        {
            var compilation = (ICompilationInternal) left.Compilation;

            return compilation.Helpers.DerivesFrom( left, right, options );
        }

        /// <summary>
        /// Determines if a type derives from another one, given as a <see cref="Type"/>.
        /// </summary>
        /// <param name="left">The child type.</param>
        /// <param name="right">The base type. It cannot be a generic type instance.</param>
        /// <param name="options">Determine with inheritance relationships should be considered.</param>
        public static bool DerivesFrom( this INamedType left, Type right, DerivedTypesOptions options = DerivedTypesOptions.Default )
        {
            var compilation = (ICompilationInternal) left.Compilation;

            return compilation.Helpers
                .DerivesFrom( left, (INamedType) compilation.Factory.GetTypeByReflectionType( right ), options );
        }

        /// <summary>
        /// Generates the <c>default(T)</c> syntax for the type.
        /// </summary>
        public static dynamic? DefaultValue( this IType type ) => ((ICompilationInternal) type.Compilation).Factory.DefaultValue( type );

        /// <summary>
        /// Gets the <see cref="AsyncInfo"/> for a type.
        /// </summary>
        /// <param name="type">Typically the return type of a method, e.g. <c>Task</c>, <c>ValueTask&lt;T&gt;</c>, <c>void</c>...</param>
        public static AsyncInfo GetAsyncInfo( this IType type ) => ((ICompilationInternal) type.Compilation).Helpers.GetAsyncInfo( type );

        /// <summary>
        /// Gets a <see cref="SerializableTypeId"/> for the type.
        /// </summary>
        public static SerializableTypeId ToSerializableId( this IType type ) => ((ICompilationInternal) type.Compilation).Helpers.GetSerializableId( type );

        /// <summary>
        /// Gets an <see cref="IExpression"/> representing 'typeof' expression for the given type.
        /// </summary>
        public static IExpression ToTypeOfExpression( this IType type ) => ((ICompilationInternal) type.Compilation).Helpers.ToTypeOfExpression( type );
    }
}