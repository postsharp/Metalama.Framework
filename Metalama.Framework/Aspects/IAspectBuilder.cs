// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Serialization;
using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// An object used by the <see cref="IAspect{T}.BuildAspect"/> method of the aspect to provide advice, child
    /// aspects and validators, or report diagnostics. This is the strongly-typed variant of the <see cref="IAspectBuilder"/> interface.
    /// </summary>
    public interface IAspectBuilder : IAspectLayerBuilder
    {
        /// <summary>
        /// Skips the application of the aspect to the code. Any provided advice is ignored, but provided children aspects
        /// and diagnostics are preserved. 
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

        IAspectBuilder<T> WithTarget<T>( T newTarget )
            where T : class, IDeclaration;
    }

    /// <summary>
    /// An object used by the <see cref="IAspect{T}.BuildAspect"/> method of the aspect to provide advice, child
    /// aspects and validators, or report diagnostics. This is a weakly-typed variant of the <see cref="IAspectBuilder{T}"/> interface.
    /// </summary>
    public interface IAspectBuilder<out TAspectTarget> : IAspectLayerBuilder<TAspectTarget>, IAspectBuilder
        where TAspectTarget : class, IDeclaration
    {
        /// <summary>
        /// Registers the build action for an aspect layer. The aspect layer must have been defined
        /// by the <see cref="LayersAttribute"/> attribute.
        /// </summary>
        /// <param name="layerName"></param>
        /// <param name="buildAction"></param>
        void BuildLayer( string? layerName, Action<IAspectLayerBuilder<TAspectTarget>> buildAction );

        /// <summary>
        /// Verifies that the target of the aspect matches an eligibility rule. If not, reports an eligibility error (unless the aspect can be used by inheritance) and skips the aspect.  
        /// </summary>
        /// <param name="rule">An eligibility rule created by <see cref="EligibilityRuleFactory"/>. For performance reasons, it is recommended that you store the rule in a static
        /// field of the aspect.</param>
        /// <returns><c>true</c> if the aspect target qualifies for the given rule, otherwise <c>false</c> (in this case, the <see cref="IAspectBuilder.SkipAspect"/> method is automatically called. </returns>
        bool VerifyEligibility( IEligibilityRule<TAspectTarget> rule );
    }
}