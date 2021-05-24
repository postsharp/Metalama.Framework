// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Validation;
using System.Collections.Generic;
using System.Threading;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// An object by the <see cref="IAspect{T}.BuildAspect"/> method of the aspect to provide advices and child
    /// aspects. This is a weakly-typed variant of the <see cref="IAspectBuilder{T}"/> interface.
    /// </summary>
    [InternalImplement]
    public interface IAspectBuilder : IValidatorAdder
    {
        IProject Project { get; }
        
        /// <summary>
        /// Gets the list of markers that have contributed to the current aspect instance to be created.
        /// </summary>
        IReadOnlyList<IAspectMarkerInstance> Markers { get; }
        
        /// <summary>
        /// Gets the list of other instances of the same type on <see cref="TargetDeclaration"/>. When several instances
        /// of the same aspect class are added to the same declaration, only the instance with the highest priority got initialized
        /// using <see cref="IAspect{T}.BuildAspect"/>. The other instances can are exposed in this property and are sorted
        /// by order of decreasing priority.
        /// </summary>
        IReadOnlyList<IAspectInstance> OtherInstances { get; }

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
        /// Gets a set of opaque properties that can be set by the aspect <see cref="IAspect{T}.BuildAspect"/> method and are then made
        /// visible in <see cref="meta.Tags"/>.
        /// </summary>
        // TODO: This is not well-defined. It may be better to expose this on IAdvice.
        IDictionary<string, object?> Tags { get; }

        CancellationToken CancellationToken { get; }
    }

    /// <summary>
    /// An object by the <see cref="IAspect{T}.BuildAspect"/> method of the aspect to provide advices and child
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