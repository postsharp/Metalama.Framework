// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Options;
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
        /// Gets the aspect type.
        /// </summary>
        IAspectClass AspectClass { get; }

        /// <summary>
        /// Gets a value indicating whether the current aspect instance has been skipped. This value is <c>true</c> if
        /// the aspect evaluation resulted in an error or if the <see cref="IAspect{T}.BuildAspect"/> method invoked
        /// <see cref="IAspectBuilder.SkipAspect"/>.
        /// </summary>
        bool IsSkipped { get; }

        bool IsInheritable { get; }

        /// <summary>
        /// Gets the other instances of the same <see cref="AspectClass"/> on the same <see cref="IAspectPredecessor.TargetDeclaration"/>.
        /// When several instances of the same <see cref="AspectClass"/> are found on the same <see cref="IAspectPredecessor.TargetDeclaration"/>,
        /// they are ordered by priority, and only the first one gets executed. The other instances are exposed on this property.
        /// </summary>
        ImmutableArray<IAspectInstance> SecondaryInstances { get; }

        /// <summary>
        /// Gets the optional opaque object defined by the aspect for the specific <see cref="IAspectPredecessor.TargetDeclaration"/> using the <see cref="IAspectBuilder.AspectState"/>
        /// property of the <see cref="IAspectBuilder"/> interface.
        /// </summary>
        IAspectState? AspectState { get; }

        /// <summary>
        /// Gets the options effective for the current aspect instance, including any options set on a parent declaration.
        /// </summary>
        /// <typeparam name="T">The type of options.</typeparam>
        T GetOptions<T>()
            where T : class, IHierarchicalOptions, new();
    }
}