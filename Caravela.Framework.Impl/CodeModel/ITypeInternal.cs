using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl
{
    internal interface ITypeInternal
    {
        ITypeSymbol TypeSymbol { get; }
    }
}
