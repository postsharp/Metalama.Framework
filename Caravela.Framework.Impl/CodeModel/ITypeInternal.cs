using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel
{
    internal interface ITypeInternal : IType
    {
        
    }

    /// <summary>
    /// Represents a type that is backed by source code, and therefore has a <see cref="TypeSymbol"/>.
    /// </summary>
    internal interface ISourceType : ITypeInternal
    {
        ITypeSymbol TypeSymbol { get; }
    }
}
