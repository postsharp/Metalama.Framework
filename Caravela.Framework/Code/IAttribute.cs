using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    /// <remarks>
    /// Values of arguments are represented as:
    /// <list type="bullet">
    /// <item>Primitive types as themselves (e.g. int as int, string as string).</item>
    /// <item>Enums as their underlying type.</item>
    /// <item><see cref="System.Type"/> as <see cref="IType"/>.</item>
    /// <item>Arrays as <c>IReadOnlyList&lt;object&gt;</c>.</item>
    /// </list>
    /// </remarks>
    public interface IAttribute
    {
        // TODO: add TargetElement?

        INamedType Type { get; }
        IMethod Constructor { get; }
        IImmutableList<object?> ConstructorArguments { get; }
        IReadOnlyDictionary<string, object?> NamedArguments { get; }
    }
}