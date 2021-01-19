using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a property or a field.
    /// </summary>
    public interface IProperty : IMember
    {
        RefKind RefKind { get; }

        // TODO: C# 10 ref fields: implement and update this documentation comment
        /// <summary>
        /// Returns <c>true</c> for <c>ref</c> and <c>ref readonly</c> properties.
        /// </summary>
        bool IsByRef { get; }

        bool IsRef { get; }

        bool IsRefReadonly { get; }

        IType Type { get; }

        IImmutableList<IParameter> Parameters { get; }

        IMethod? Getter { get; }

        // TODO: what happens if you try to set a get-only property in a constructor? it works, Setter returns pseudo elements for get-only
        // IsPseudoElement
        IMethod? Setter { get; }
    }
}