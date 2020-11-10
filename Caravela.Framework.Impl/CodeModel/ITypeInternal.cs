using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal interface ITypeInternal
    {
        ITypeSymbol TypeSymbol { get; }
    }
}
