// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        private struct ForcefullyInitializedType
        {
            public IntermediateSymbolSemantic<IMethodSymbol>[] Constructors { get; }

            public ISymbol[] InitializedSymbols { get; }

            public ForcefullyInitializedType( IntermediateSymbolSemantic<IMethodSymbol>[] constructors, ISymbol[] initializedSymbols )
            {
                this.Constructors = constructors;
                this.InitializedSymbols = initializedSymbols;
            }
        }
    }
}