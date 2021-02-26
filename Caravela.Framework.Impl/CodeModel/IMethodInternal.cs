// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    /// <summary>
    /// Provides additional properties and methods for <see cref="IMethod"/> code element which are not exposed in the public API.
    /// </summary>
    internal interface IMethodInternal : IMethod
    {
        /// <summary>
        /// Finds all the symbols that are accessible and visible at the beginning of the method body.
        /// For a method without a body finds all the symbols at the location of the method declaration.
        /// </summary>
        /// <returns>
        /// A read-only list of symbols visible at the beginning of the method body or method declaration.
        /// </returns>
        IReadOnlyList<ISymbol> LookupSymbols();
    }
}
