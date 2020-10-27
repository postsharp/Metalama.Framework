using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    interface ISymbolClassifier
    {
        SymbolDeclarationScope GetSymbolDeclarationScope( ISymbol symbol );
    }
}
