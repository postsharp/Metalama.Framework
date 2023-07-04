// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Describes conversion between types possible during comparison.
    /// </summary>
    public enum ConversionKind
    {
        /// <summary>
        /// Accepts any value implicitly convertible to the given type, including boxing and user-defined implicit operators. 
        /// </summary>
        Default,

        /// <summary>
        /// Accepts any value that is reference-compatible with the given type i.e. instances of subclasses or interface implementations, but refuses boxing conversions.
        /// </summary>
        Reference,

        /// <summary>
        /// Accepts any value that is type-compatible, including boxing conversions, but excluding user-defined implicit operators. This corresponds to the bbehavior of C# <c>is</c> operator.
        /// </summary>
        ReferenceOrBoxing,

        /// <summary>
        /// Accepts any value that extends or implements a type that is of the same type definition as the given type definition.
        /// </summary>
        /// <remarks>
        /// For non-generic types behaves like <see cref="Reference"/>. For generic types, ignores all type arguments and tests that 
        /// the given type definition is equal to type definition of the value, type definition of any base type, or type definition of
        /// any implemented interface (by type itself or by any base type, including base interfaces).
        /// </remarks>
        TypeDefinition,

        [Obsolete( "Use Reference.", true )]
        ImplicitReference = Reference,

        [Obsolete( "Use Default.", true )]
        Implicit = Default
    }
}