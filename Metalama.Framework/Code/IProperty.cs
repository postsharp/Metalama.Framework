// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represents a property.
    /// </summary>
    /// <seealso cref="IIndexer"/>
    public interface IProperty : IFieldOrProperty, IPropertyOrIndexer
    {
        /// <summary>
        /// Gets a list of interface properties this property explicitly implements.
        /// </summary>
        IReadOnlyList<IProperty> ExplicitInterfaceImplementations { get; }

        /// <summary>
        /// Gets the base property that is overridden by the current property.
        /// </summary>
        IProperty? OverriddenProperty { get; }
    }
}