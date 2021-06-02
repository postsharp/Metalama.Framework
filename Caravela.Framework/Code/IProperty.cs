// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System.Collections.Generic;
using System.Reflection;

#pragma warning disable SA1623 // Property summary documentation should match accessors

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents a property.
    /// </summary>
    public interface IProperty : IFieldOrProperty, IHasParameters
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
        /// Gets a list of interface properties this property explicitly implements.
        /// </summary>
        IReadOnlyList<IProperty> ExplicitInterfaceImplementations { get; }

        /// <summary>
        /// Gets a <see cref="PropertyInfo"/> that represents the current property at run time.
        /// </summary>
        /// <returns>A <see cref="PropertyInfo"/> that can be used only in run-time code.</returns>
        [return: RunTimeOnly]
        PropertyInfo ToPropertyInfo();

        new IPropertyInvoker? BaseInvoker { get; }

        new IPropertyInvoker Invoker { get; }
    }
}