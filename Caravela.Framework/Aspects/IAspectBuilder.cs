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
    /// An object used by the <see cref="IAspect{T}.BuildAspect"/> method of the aspect to provide advices, child
    /// aspects and validators, or report diagnostics. This is a weakly-typed variant of the <see cref="IAspectBuilder{T}"/> interface.
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
    }

    /// <summary>
    /// An object used by the <see cref="IAspect{T}.BuildAspect"/> method of the aspect to provide advices, child
    /// aspects and validators, or report diagnostics. This is the strongly-typed variant of the <see cref="IAspectBuilder"/> interface.
    /// </summary>
    public interface IAspectBuilder<out TAspectTarget> : IAspectBuilder
        where TAspectTarget : IDeclaration
    {
        /// <summary>
        /// Gets the declaration to which the aspect was added.
        /// </summary>
        new TAspectTarget TargetDeclaration { get; }

        /// <summary>
        /// Selects members of the current target declaration with the purpose of adding aspects and annotations to them
        /// using e.g. <see cref="IDeclarationSelection{TDeclaration}.AddAspect{TAspect}(System.Linq.Expressions.Expression{System.Func{TDeclaration,TAspect}})"/>
        /// or <see cref="IDeclarationSelection{TDeclaration}.AddAnnotation{TAspect,TAnnotation}"/>.
        /// </summary>
        /// <param name="selector"></param>
        /// <typeparam name="TMember"></typeparam>
        /// <returns></returns>
        [Obsolete( "Not implemented." )]
        IDeclarationSelection<TMember> WithMembers<TMember>( Func<TAspectTarget, TMember> selector )
            where TMember : class, IDeclaration;
    }
}