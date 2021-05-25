// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Threading;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Provides the <see cref="GetCompileTimeType"/> method, which maps a Roslyn <see cref="ITypeSymbol"/> to a reflection <see cref="Type"/>.
    /// </summary>
    internal interface ICompileTimeTypeResolver
    {
        /// <summary>
        /// Maps a Roslyn <see cref="ITypeSymbol"/> to a reflection <see cref="Type"/>. 
        /// </summary>
        Type? GetCompileTimeType( ITypeSymbol typeSymbol, bool fallbackToMock, CancellationToken cancellationToken = default );
    }
}