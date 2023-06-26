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
        /// Accepts implicit conversion, including boxing and custom implicit operators, as does the C# <c>is</c> operator. 
        /// </summary>
        Default,

        /// <summary>
        /// Accepts any type-compatible value without boxing (unlike the C# <c>is</c> operator).
        /// </summary>
        DenyBoxing,

        /// <summary>
        /// Accepts any value with the same generic type definition.
        /// </summary>
        IgnoreTypeArguments,

        [Obsolete( "Use DenyBoxing.", true )]
        ImplicitReference = DenyBoxing,

        [Obsolete( "Use Default.", true )]
        Implicit = Default,
    }
}