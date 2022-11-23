// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        private struct ForcefullyInitializedType
        {
            public INamedTypeSymbol Type { get; }

            public IntermediateSymbolSemantic<IMethodSymbol>[] Constructors { get; }

            public ISymbol[] InitializedSymbols { get; }

            public ForcefullyInitializedType( INamedTypeSymbol type, IntermediateSymbolSemantic<IMethodSymbol>[] constructors, ISymbol[] initializedSymbols )
            {
                this.Type = type;
                this.Constructors = constructors;
                this.InitializedSymbols = initializedSymbols;
            }
        }
    }
}