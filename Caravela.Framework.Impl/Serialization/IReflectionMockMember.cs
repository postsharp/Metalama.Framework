using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Serialization.Reflection
{
    internal interface IReflectionMockMember : IReflectionMockCodeElement
    {

        ITypeSymbol? DeclaringTypeSymbol { get; }
    }
}