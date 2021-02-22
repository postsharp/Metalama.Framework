using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.ReflectionMocks
{
    internal interface IReflectionMockCodeElement
    {
        ISymbol Symbol { get; }
    }
}