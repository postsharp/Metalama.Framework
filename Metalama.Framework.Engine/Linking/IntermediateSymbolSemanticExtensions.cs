// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Linking
{
    internal static class IntermediateSymbolSemanticExtensions
    {
        public static IntermediateSymbolSemantic<T> ToSemantic<T>( this T symbol, IntermediateSymbolSemanticKind kind )
            where T : ISymbol
        {
            return ((ISymbol) symbol).ToSemantic( kind ).ToTyped<T>();
        }

        public static IntermediateSymbolSemantic ToSemantic( this ISymbol symbol, IntermediateSymbolSemanticKind kind )
        {
            return new IntermediateSymbolSemantic( symbol, kind );
        }
    }
}