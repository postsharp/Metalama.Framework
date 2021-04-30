// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal interface ICompileTimeReflectionObject
    {
        // TODO: We won't have a symbol for introduced members.
        ISymbol Symbol { get; }
    }
}