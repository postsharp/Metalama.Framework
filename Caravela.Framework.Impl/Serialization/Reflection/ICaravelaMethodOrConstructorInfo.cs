using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal interface ICaravelaMethodOrConstructorInfo
    {
        ISymbol Symbol { get; }

        ITypeSymbol? DeclaringTypeSymbol { get; }
    }
}