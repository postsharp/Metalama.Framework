// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Validation;
using System.Collections.Immutable;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Represents an instance of an aspect. The instance of the <see cref="IAspect"/> itself is in the <see cref="Aspect"/> property.
    /// </summary>
    [InternalImplement]
    [CompileTimeOnly]
    public interface IAspectInstance : IAspectPredecessor
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

        /// <summary>
        /// Gets a value indicating whether the current aspect instance has been skipped. This value is <c>true</c> if
        /// the aspect evaluation resulted in an error or if the <see cref="IAspect{T}.BuildAspect"/> method invoked
        /// <see cref="IAspectBuilder.SkipAspect"/>.
        /// </summary>
        bool IsSkipped { get; }

        /// <summary>
        /// Gets the other instances of the same <see cref="AspectClass"/> on the same <see cref="TargetDeclaration"/>.
        /// When several instances of the same <see cref="AspectClass"/> are found on the same <see cref="TargetDeclaration"/>,
        /// they are ordered by priority, and only the first one gets executed. The other instances are exposed on this property.
        /// </summary>
        ImmutableArray<IAspectInstance> OtherInstances { get; }

        /// <summary>
        /// Gets the list of objects that have caused the current aspect instance (but not any instance in the <see cref="OtherInstances"/> list)
        /// to be created.
        /// </summary>
        ImmutableArray<AspectPredecessor> Predecessors { get; }
    }
}