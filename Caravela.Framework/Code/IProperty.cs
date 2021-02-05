using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    public interface IPropertyInvocation
    {
        dynamic Value { get; set; }

        /// <summary>
        /// Get the value for a different instance;
        /// </summary>
        dynamic GetValue( dynamic? instance );

        /// <summary>
        /// Set the value for a different instance;
        /// </summary>
        dynamic SetValue( dynamic? instance, dynamic value );

        /// <summary>
        /// Get the value for an indexer.
        /// </summary>
        dynamic GetIndexerValue( dynamic? instance, params dynamic[] args );

        /// <summary>
        /// Set the value for an indexer.
        /// </summary>
        /// <remarks>
        /// Note: the order of parameters is different than in C# code:
        /// e.g. <c>instance[args] = value</c> is <c>indexer.SetIndexerValue(instance, value, args)</c>.
        /// </remarks>
        dynamic SetIndexerValue( dynamic? instance, dynamic value, params dynamic[] args );
    }

    /// <summary>
    /// Represents a property or a field.
    /// </summary>
    public interface IProperty : IMember, IPropertyInvocation
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

        /// <summary>
        /// Determines if the method existed before the current aspect was advice
        /// (<see langword="false" /> if it was introduced by the current aspect).
        /// </summary>
        bool HasBase { get; }

        /// <summary>
        /// Allows invocation of the base method (<see langword="null" /> if the method was introduced by the current aspect).
        /// </summary>
        IPropertyInvocation Base { get; }
    }
}