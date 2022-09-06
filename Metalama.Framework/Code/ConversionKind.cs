// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Code
{
    public enum ConversionKind
    {
        /// <summary>
        /// Means <see cref="Implicit"/>.
        /// </summary>
        Default,

        /// <summary>
        /// Accepts any type-compatible value without boxing (unlike the C# <c>is</c> operator).
        /// </summary>
        ImplicitReference,

        /// <summary>
        /// Accepts implicit conversion, including boxing and custom implicit operators, as does the C# <c>is</c> operator. 
        /// </summary>
        Implicit = Default
    }
}