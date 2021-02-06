using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    internal interface ISymbolClassifier
    {
        SymbolDeclarationScope GetSymbolDeclarationScope( ISymbol symbol );
    }
}
