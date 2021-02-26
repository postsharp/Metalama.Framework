// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    internal interface ICompileTimeTypeResolver
    {
        Type? GetCompileTimeType( ITypeSymbol typeSymbol, bool fallbackToMock );
    }
}