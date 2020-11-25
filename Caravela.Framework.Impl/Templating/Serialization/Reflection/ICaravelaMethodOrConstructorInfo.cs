using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Templating.Serialization.Reflection
{
    internal interface ICaravelaMethodOrConstructorInfo
    {
        ISymbol Symbol { get; }
        ISymbol DeclaringTypeSymbol { get; }
    }
}