using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel;

internal interface ISymbolBasedCompilationElement : ICompilationElementImpl
{
    ISymbol Symbol { get; }
}