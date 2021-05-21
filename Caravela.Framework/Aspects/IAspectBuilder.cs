// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using System.Collections.Generic;
using System.Threading;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// An object by the <see cref="IAspect{T}.Initialize"/> method of the aspect to provide advices and child
    /// aspects. This is a weakly-typed variant of the <see cref="IAspectBuilder{T}"/> interface.
    /// </summary>
    public interface IAspectBuilder
    {
        /// <summary>
        /// Gets a service that allows to report or suppress diagnostics.
        /// </summary>
        IDiagnosticSink Diagnostics { get; }

        /// <summary>
        /// Gets the declaration to which the aspect was added.
        /// </summary>
        IDeclaration TargetDeclaration { get; }

        /// <summary>
        /// Gets an object that exposes methods that allow to create advices.
        /// </summary>
        IAdviceFactory AdviceFactory { get; }

        /// <summary>
        /// Skips the application of the aspect to the code. Any provided advice is ignored, but provided children aspects
        /// and diagnostics are preserved. 
        /// </summary>
        /// <remarks>
        /// Note that reporting an error using
        /// <see cref="IDiagnosticSink.Report"/>
        /// automatically causes the aspect to be skipped, but, additionally, provided children aspects are ignored.
        /// </remarks>
        void SkipAspect();

        /// <summary>
        /// Gets a set of opaque properties that can be set by the aspect <see cref="IAspect{T}.Initialize"/> method and are then made
        /// visible in <see cref="meta.Tags"/>.
        /// </summary>
        IDictionary<string, object?> Tags { get; }

        CancellationToken CancellationToken { get; }
    }

    /// <summary>
    /// An object by the <see cref="IAspect{T}.Initialize"/> method of the aspect to provide advices and child
    /// aspects. This is the strongly-typed variant of the <see cref="IAspectBuilder"/> interface.
    /// </summary>
    public interface IAspectBuilder<out T> : IAspectBuilder
        where T : IDeclaration
    {
        /// <summary>
        /// Gets the declaration to which the aspect was added.
        /// </summary>
        new T TargetDeclaration { get; }
    }
}