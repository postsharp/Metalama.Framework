// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Validation;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Represents an instance of an aspect (the CLR instance itself is in the <see cref="Aspect"/> property.
    /// </summary>
    [InternalImplement]
    [CompileTimeOnly]
    public interface IAspectInstance
    {
        /// <summary>
        /// Gets the aspect instance.
        /// </summary>
        IAspect Aspect { get; }

        /// <summary>
        /// Gets the declaration to which the aspect is applied.
        /// </summary>
        IDeclaration TargetDeclaration { get; }

        /// <summary>
        /// Gets the aspect type.
        /// </summary>
        IAspectClass AspectClass { get; }
    }
}