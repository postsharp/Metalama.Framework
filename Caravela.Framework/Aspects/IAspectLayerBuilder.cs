// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;
using Caravela.Framework.Project;
using Caravela.Framework.Validation;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// An object used by the delegated passed to <see cref="IAspectBuilder{TAspectTarget}.SetAspectLayerBuildAction"/> method of the aspect to provide advices, child
    /// aspects and validators, or report diagnostics. This is a weakly-typed variant of the <see cref="IAspectLayerBuilder{T}"/> interface.
    /// </summary>
    [InternalImplement]
    public interface IAspectLayerBuilder : IValidatorAdder
    {
        IProject Project { get; }

        /// <summary>
        /// Gets the list of aspects that have required this aspect to be created.
        /// </summary>
        [Obsolete( "Not implemented." )]
        IReadOnlyList<IAspectInstance> UpstreamAspects { get; }

        /// <summary>
        /// Gets the list of other instances of the same type on <see cref="Target"/>. When several instances
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
        IDeclaration Target { get; }

        /// <summary>
        /// Gets an object that allows to create advices, e.g. overriding members, introducing members, or implementing new interfaces.
        /// </summary>
        IAdviceFactory Advices { get; }

        CancellationToken CancellationToken { get; }
    }

    /// <summary>
    /// An object used by the delegated passed to <see cref="IAspectBuilder{TAspectTarget}.SetAspectLayerBuildAction"/> method of the aspect to provide advices, child
    /// aspects and validators, or report diagnostics. This is the strongly-typed variant of the <see cref="IAspectLayerBuilder"/> interface.
    /// </summary>
    public interface IAspectLayerBuilder<out TAspectTarget> : IAspectLayerBuilder
        where TAspectTarget : IDeclaration
    {
        /// <summary>
        /// Gets the declaration to which the aspect was added.
        /// </summary>
        new TAspectTarget Target { get; }

        /// <summary>
        /// Selects members of the current target declaration with the purpose of adding aspects and annotations to them
        /// using e.g. <see cref="IDeclarationSelection{TDeclaration}.AddAspect{TAspect}(System.Func{TDeclaration,System.Linq.Expressions.Expression{System.Func{TAspect}}})"/>
        /// or <see cref="IDeclarationSelection{TDeclaration}.AddAnnotation{TAspect,TAnnotation}"/>.
        /// </summary>
        /// <param name="selector"></param>
        /// <typeparam name="TMember"></typeparam>
        /// <returns></returns>
        IDeclarationSelection<TMember> WithMembers<TMember>( Func<TAspectTarget, IEnumerable<TMember>> selector )
            where TMember : class, IDeclaration;
    }
}