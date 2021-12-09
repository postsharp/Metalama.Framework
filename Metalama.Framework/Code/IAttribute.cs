// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code
{
    /// <summary>
    /// Represent a custom attributes.
    /// </summary>
    /// <seealso cref="AttributeExtensions"/>
    public interface IAttribute : IDeclaration, IAttributeData, IAspectPredecessor
    {
        /// <summary>
        /// Gets the declaration that owns the custom attribute.
        /// </summary>
        new IDeclaration ContainingDeclaration { get; }
    }
}