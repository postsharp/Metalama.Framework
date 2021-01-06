using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    public interface IProperty : IMember
    {
        // TODO: ref
        IType Type { get; }
        IImmutableList<IParameter> Parameters { get; }
        IMethod? Getter { get; }
        // TODO: what happens if you try to set a get-only property in a constructor? it works, Setter returns pseudo elements for get-only
        // IsPseudoElement
        IMethod? Setter { get; }
    }
}