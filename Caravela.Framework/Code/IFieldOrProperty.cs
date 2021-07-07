// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.RunTime;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// A base interface for <see cref="IField"/> and <see cref="IProperty"/>.
    /// </summary>
    public interface IFieldOrProperty : IMember
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
        /// Gets writeability of the field or property, i.e. the situations in which the field or property can be written.
        /// </summary>
        Writeability Writeability { get; }

        /// <summary>
        /// Gets a value indicating whether the declaration is an auto-property or a field.
        /// </summary>
        bool IsAutoPropertyOrField { get; }

        /// <summary>
        /// Gets an object that allows to get or set the value of the current field or property.
        /// </summary>
        IInvokerFactory<IFieldOrPropertyInvoker> Invokers { get; }

        /// <summary>
        /// Gets a <see cref="FieldOrPropertyInfo"/> that represents the current field or property at run time.
        /// </summary>
        /// <returns>A <see cref="FieldOrPropertyInfo"/> that can be used only in run-time code.</returns>
        [return: RunTimeOnly]
        FieldOrPropertyInfo ToFieldOrPropertyInfo();
    }
}