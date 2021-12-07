// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;
using System.Collections.Immutable;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// An object used by implementations of the <see cref="IAspect.BuildAspectClass"/> method to set the static characteristics of
    /// the aspect class, i.e. those that do not depend on the aspect instance state.
    /// </summary>
    [InternalImplement]
    [CompileTimeOnly]
    public interface IAspectClassBuilder
    {
        /// <summary>
        /// Gets or sets a value indicating whether the aspect can be used as a live template. A live template is a code transformation
        /// that is applied in the IDE to the source code. A live template must have a default constructor.
        /// </summary>
        bool IsLiveTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the aspect can be inherited.
        /// </summary>
        bool IsInherited { get; set; }

        /// <summary>
        /// Gets or sets the name of the aspect as shown to the user at design time.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description of the aspect.
        /// </summary>
        string? Description { get; set; }

        /// <summary>
        /// Gets or sets the list of layers of the aspect. Layers must be sorted by order of execution (e.g. inverse order of application).
        /// Layers must then be initialized in the <see cref="IAspect{T}.BuildAspect"/> method using
        /// <see cref="IAspectBuilder{TAspectTarget}.SetAspectLayerBuildAction"/> method.
        /// </summary>
        ImmutableArray<string> Layers { get; set; }

        /// <summary>
        /// Gets a service allowing to specify the dependencies of the current aspect class.
        /// </summary>
        IAspectDependencyBuilder Dependencies { get; }
    }
}