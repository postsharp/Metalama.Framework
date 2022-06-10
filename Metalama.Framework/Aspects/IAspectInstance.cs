﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using System.Collections.Immutable;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Represents an instance of an aspect. The instance of the <see cref="IAspect"/> itself is in the <see cref="Aspect"/> property.
    /// </summary>
    [InternalImplement]
    [CompileTime]
    public interface IAspectInstance : IAspectPredecessor
    {
        /// <summary>
        /// Gets the aspect instance.
        /// </summary>
        IAspect Aspect { get; }

        /// <summary>
        /// Gets the declaration to which the aspect is applied.
        /// </summary>
        IRef<IDeclaration> TargetDeclaration { get; }

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
        ImmutableArray<IAspectInstance> SecondaryInstances { get; }

        /// <summary>
        /// Gets the list of objects that have caused the current aspect instance (but not any instance in the <see cref="SecondaryInstances"/> list)
        /// to be created.
        /// </summary>
        /// <seealso href="@child-aspects"/>
        ImmutableArray<AspectPredecessor> Predecessors { get; }

        /// <summary>
        /// Gets the optional opaque object defined by the aspect for the specific <see cref="TargetDeclaration"/> using the <see cref="IAspectBuilder.AspectState"/>
        /// property of the <see cref="IAspectBuilder"/> interface.
        /// </summary>
        IAspectState? State { get; }
    }
}