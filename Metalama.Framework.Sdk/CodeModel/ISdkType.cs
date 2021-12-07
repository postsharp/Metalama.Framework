// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
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