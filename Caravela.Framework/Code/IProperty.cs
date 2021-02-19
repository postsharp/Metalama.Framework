using System.Collections.Generic;

#pragma warning disable SA1623 // Property summary documentation should match accessors

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a property or a field.
    /// </summary>
    public interface IProperty : IMember, IPropertyInvocation
    {

        /// <summary>
        /// Gets the <c>in</c>, <c>ref</c>, <c>ref readonly</c> property type modifier.
        /// </summary>
        RefKind RefKind { get; }

        // TODO: C# 10 ref fields: implement and update this documentation comment

        /// <summary>
        /// Returns <c>true</c> for <c>ref</c> and <c>ref readonly</c> properties.
        /// </summary>
        bool IsByRef { get; }

        /// <summary>
        /// Returns <c>true</c> for <c>ref</c> but <c>false</c> for <c>ref readonly</c> properties.
        /// </summary>
        bool IsRef { get; }

        /// <summary>
        /// Returns <c>true</c> for <c>ref readonly</c> properties.
        /// </summary>
        bool IsRefReadonly { get; }

        /// <summary>
        /// Gets the property type.
        /// </summary>
        IType Type { get; }

        /// <summary>
        /// Gets the list of parameters of the property, if it is an indexer.
        /// </summary>
        IParameterList Parameters { get; }

        /// <summary>
        /// Gets the property getter, or <c>null</c> if the property is write-only. In case of automatic properties, this property returns
        /// an object that does not map to source code but allows to add aspects and advices as with a normal method.
        /// </summary>
        IMethod? Getter { get; }

        /// <summary>
        /// Gets the property getter, or <c>null</c> if the property is read-only. In case of automatic properties, this property returns
        /// an object that does not map to source code but allows to add aspects and advices as with a normal method.
        /// </summary>
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