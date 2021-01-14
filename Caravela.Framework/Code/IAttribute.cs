using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    public interface IAttribute
    {
        // TODO: add TargetElement?

        INamedType Type { get; }
        IImmutableList<object?> ConstructorArguments { get; }
        IReadOnlyDictionary<string, object?> NamedArguments { get; }
    }
}