// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel
{
    /// <summary>
    /// Extends the user-level <see cref="IType"/> interface with a <see cref="TypeSymbol"/> exposing the Roslyn <see cref="ITypeSymbol"/>.
    /// </summary>
    internal interface ISdkType : IType
    {
        /// <summary>
        /// Gets the <see cref="ITypeSymbol"/> for the current type.
        /// </summary>
        ITypeSymbol? TypeSymbol { get; }
    }
}