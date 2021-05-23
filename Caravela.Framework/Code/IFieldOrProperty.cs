// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Project;

#pragma warning disable SA1623 // Property summary documentation should match accessors

namespace Caravela.Framework.Code
{
    /// <summary>
    /// A base interface for <see cref="IField"/> and <see cref="IProperty"/>.
    /// </summary>
    public interface IFieldOrProperty : IMember, IFieldOrPropertyInvocation
    {
        /// <summary>
        /// Gets the field or property type.
        /// </summary>
        IType Type { get; }

        /// <summary>
        /// Gets the property getter, or <c>null</c> if the property is write-only. In case of automatic properties, this property returns
        /// an object that does not map to source code but allows to add aspects and advices as with a normal method. In case of fields,
        /// this property returns a pseudo-method that can be the target of aspects and advices, as if the field were a property.
        /// </summary>
        IMethod? Getter { get; }

        /// <summary>
        /// Gets the property getter, or <c>null</c> if the property is read-only. In case of automatic properties, this property returns
        /// an object that does not map to source code but allows to add aspects and advices as with a normal method. In case of fields,
        /// this property returns a pseudo-method that can be the target of aspects and advices, as if the field were a property.
        /// </summary>
        IMethod? Setter { get; }

        /// <summary>
        /// Determines if the property existed before the current aspect was advice
        /// (<see langword="false" /> if it was introduced by the current aspect).
        /// </summary>
        bool HasBase { get; }

        /// <summary>
        /// Allows invocation of the base property (<see langword="null" /> if the method was introduced by the current aspect).
        /// </summary>
        IFieldOrPropertyInvocation Base { get; }

        /// <summary>
        /// Gets a <see cref="FieldOrPropertyInfo"/> that represents the current field or property at run time.
        /// </summary>
        /// <returns>A <see cref="FieldOrPropertyInfo"/> that can be used only in run-time code.</returns>
        [return: RunTimeOnly]
        FieldOrPropertyInfo ToFieldOrPropertyInfo();
    }
}