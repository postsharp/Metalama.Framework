// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Project;
using Metalama.Framework.Serialization;
using System.Threading;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// An object used by the <see cref="IAspect{T}.BuildAspect"/> method of the aspect to provide advice, child
    /// aspects and validators, or report diagnostics. This is the strongly-typed variant of the <see cref="IAspectBuilder"/> interface.
    /// </summary>
    [CompileTime]
    public interface IAspectBuilder
    {
        /// <summary>
        /// Gets the current <see cref="IProject"/>, which represents the <c>csproj</c> file and allows to share project-local data.
        /// </summary>
        IProject Project { get; }

        /// <summary>
        /// Gets the current <see cref="IAspectInstance"/>, which gives access to the <see cref="IAspectInstance.Predecessors"/>
        /// and the <see cref="IAspectInstance.SecondaryInstances"/> of the current aspect.
        /// </summary>
        IAspectInstance AspectInstance { get; }

        /// <summary>
        /// Gets a service that allows to report or suppress diagnostics.
        /// </summary>
        ScopedDiagnosticSink Diagnostics { get; }

        /// <summary>
        /// Gets the declaration to which the aspect was added.
        /// </summary>
        IDeclaration Target { get; }

        /// <summary>
        /// Gets an object that allows to create an advice, e.g. overriding members, introducing members, or implementing new interfaces.
        /// </summary>
        IAdviceFactory Advice { get; }

        /// <summary>
        /// Gets the cancellation token for the current operation.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Skips the application of the aspect to the code. Any provided advice is ignored, but provided children aspects
        /// and diagnostics are preserved. In multi-layer aspects, the next layers of the aspect are skipped. 
        /// </summary>
        /// <remarks>
        /// Note that reporting an error automatically causes the aspect to be skipped, but, additionally, provided children aspects are ignored.
        /// </remarks>
        void SkipAspect();

        /// <summary>
        /// Gets a value indicating whether the <see cref="SkipAspect"/> method was called.
        /// </summary>
        bool IsAspectSkipped { get; }

        /// <summary>
        /// Gets or sets an arbitrary object that is then exposed on the <see cref="IAspectInstance.AspectState"/> property of
        /// the <see cref="IAspectInstance"/> interface. While a single instance of an aspect class can be used for
        /// several target declarations, the <see cref="AspectState"/> is specific to the target declaration. If the aspect
        /// is inherited, the <see cref="AspectState"/> must be lama-serializable (<see cref="ILamaSerializable"/> or
        /// default serializable classes).
        /// </summary>
        IAspectState? AspectState { get; set; }

        /// <summary>
        /// Gets the name of the layer being built, or <c>null</c> if this is the default (initial) layer.
        /// When an aspect has several layers, the <see cref="IAspect{T}.BuildAspect"/> method is called several times. To register
        /// aspect layers, add the <see cref="LayersAttribute"/> custom attribute to the aspect class.
        /// </summary>
        string? Layer { get; }

        /// <summary>
        /// Returns a copy of the current <see cref="IAspectBuilder"/>, for use in the current execution context,
        /// but for a different <see cref="Target"/> declaration.
        /// </summary>
        IAspectBuilder<T> WithTarget<T>( T newTarget )
            where T : class, IDeclaration;
    }

    /// <summary>
    /// An object used by the <see cref="IAspect{T}.BuildAspect"/> method of the aspect to provide advice, child
    /// aspects and validators, or report diagnostics. This is a weakly-typed variant of the <see cref="IAspectBuilder{T}"/> interface.
    /// </summary>
    public interface IAspectBuilder<out TAspectTarget> : IAspectBuilder, IAspectReceiverSelector<TAspectTarget>
        where TAspectTarget : class, IDeclaration
    {
        /// <summary>
        /// Verifies that the target of the aspect matches an eligibility rule. If not, reports an eligibility error (unless the aspect can be used by inheritance) and skips the aspect.  
        /// </summary>
        /// <param name="rule">An eligibility rule created by <see cref="EligibilityRuleFactory"/>. For performance reasons, it is recommended that you store the rule in a static
        /// field of the aspect.</param>
        /// <returns><c>true</c> if the aspect target qualifies for the given rule, otherwise <c>false</c> (in this case, the <see cref="IAspectBuilder.SkipAspect"/> method is automatically called. </returns>
        bool VerifyEligibility( IEligibilityRule<TAspectTarget> rule );

        /// <summary>
        /// Gets the declaration to which the aspect was added.
        /// </summary>
        new TAspectTarget Target { get; }
    }
}