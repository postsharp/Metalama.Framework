// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Represents an instance of an aspect (the CLR instance itself is in the <see cref="Aspect"/> property.
    /// </summary>
    public interface IAspectInstance
    {
        /// <summary>
        /// Gets the aspect instance.
        /// </summary>
        IAspect Aspect { get; }

        /// <summary>
        /// Gets the element of code to which the aspect is applied.
        /// </summary>
        IDeclaration Declaration { get; }

        /// <summary>
        /// Gets the aspect type.
        /// </summary>
        IAspectClassMetadata AspectClass { get; }
    }
}