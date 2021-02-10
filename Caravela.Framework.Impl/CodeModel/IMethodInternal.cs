using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal interface IMethodInternal
    {
        IReadOnlyList<ISymbol> LookupSymbols();
    }
}
