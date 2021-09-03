// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Linking
{
    internal static class IntermediateSymbolSemanticExtensions
    {
        public static IntermediateSymbolSemantic<T> ToSemantic<T>(this T symbol, IntermediateSymbolSemanticKind kind)
            where T : ISymbol
        {
            return ((ISymbol) symbol).ToSemantic(kind).ToTyped<T>();
        }

        public static IntermediateSymbolSemantic ToSemantic( this ISymbol symbol, IntermediateSymbolSemanticKind kind )
        {
            return new IntermediateSymbolSemantic( symbol, kind );
        }
    }
}