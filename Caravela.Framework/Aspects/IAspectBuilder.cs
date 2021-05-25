// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Validation;
using System;
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
        [Obsolete( "Not implemented." )]
        IProject Project { get; }

        /// <summary>
        /// Gets the list of aspects that have required this aspect to be created.
        /// </summary>
        [Obsolete( "Not implemented." )]
        IReadOnlyList<IAspectInstance> UpstreamAspects { get; }

        /// <summary>
        /// Gets the list of other instances of the same type on <see cref="TargetDeclaration"/>. When several instances
        /// of the same aspect class are added to the same declaration, only the instance with the highest priority got initialized
        /// using <see cref="IAspect{T}.BuildAspect"/>. The other instances can are exposed in this property and are sorted
        /// by order of decreasing priority.
        /// </summary>
        [Obsolete( "Not implemented." )]
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

        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Requires an instance of a specified aspect type to be present on a specified declaration. If the aspect
        /// is not present, this method adds a new instance of this aspect by using the default aspect constructor.
        /// </summary>
        /// <remarks>
        /// <para>Calling this method causes the current aspect to be present in the <see cref="UpstreamAspects"/> list
        /// even if the required aspect was already present on the target declaration.</para>
        /// </remarks>
        /// <param name="target">The target declaration. It must be contained in the current type.</param>
        /// <typeparam name="TTarget">Type of the target declaration.</typeparam>
        /// <typeparam name="TAspect">Type of the aspect. The type must be ordered after the current aspect type.</typeparam>
        [Obsolete( "Not implemented." )]
        void RequireAspect<TTarget, TAspect>( TTarget target )
            where TTarget : class, IDeclaration
            where TAspect : IAspect<TTarget>, new();
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