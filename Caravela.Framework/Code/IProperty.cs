// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code.Invokers;
using System.Collections.Generic;
using System.Reflection;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a property.
    /// </summary>
    public interface IProperty : IFieldOrProperty, IHasParameters, IMemberWithAccessors
    {
        /// <summary>
        /// Gets the <c>in</c>, <c>ref</c>, <c>ref readonly</c> property type modifier.
        /// </summary>
        RefKind RefKind { get; }

        /// <summary>
        /// Gets a list of interface properties this property explicitly implements.
        /// </summary>
        IReadOnlyList<IProperty> ExplicitInterfaceImplementations { get; }

        /// <summary>
        /// Gets a <see cref="PropertyInfo"/> that represents the current property at run time.
        /// </summary>
        /// <returns>A <see cref="PropertyInfo"/> that can be used only in run-time code.</returns>
        PropertyInfo ToPropertyInfo();

        /// <summary>
        /// Gets an object that allows to invoke the current property.
        /// </summary>
        new IInvokerFactory<IPropertyInvoker> Invokers { get; }

        /// <summary>
        /// Gets the base property that is overridden by the current property.
        /// </summary>
        IProperty? OverriddenProperty { get; }
    }
}