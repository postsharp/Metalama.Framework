using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal interface ITypeInternal : IType
    {
        ITypeSymbol TypeSymbol { get; }
    }
}
