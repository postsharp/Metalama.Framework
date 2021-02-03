using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel
{
    internal interface IMethodInternal
    {
        IReadOnlyList<ISymbol> LookupSymbols();
    }
}
