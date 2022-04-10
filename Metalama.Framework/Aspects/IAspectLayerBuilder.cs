// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Project;
using Metalama.Framework.Validation;
using System.Threading;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// An object used by the delegated passed to <see cref="IAspectBuilder{TAspectTarget}.SetAspectLayerBuildAction"/> method of the aspect to provide advices, child
    /// aspects and validators, or report diagnostics. This is a weakly-typed variant of the <see cref="IAspectLayerBuilder{T}"/> interface.
    /// </summary>
    [InternalImplement]
    [CompileTime]
    public interface IAspectLayerBuilder
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
        /// Gets an object that allows to create advices, e.g. overriding members, introducing members, or implementing new interfaces.
        /// </summary>
        IAdviceFactory Advices { get; }

        /// <summary>
        /// Gets the cancellation token for the current operation.
        /// </summary>
        CancellationToken CancellationToken { get; }
    }

    /// <summary>
    /// An object used by the delegated passed to <see cref="IAspectBuilder{TAspectTarget}.SetAspectLayerBuildAction"/> method of the aspect to provide advices, child
    /// aspects and validators, or report diagnostics. This is the strongly-typed variant of the <see cref="IAspectLayerBuilder"/> interface.
    /// </summary>
    public interface IAspectLayerBuilder<out TAspectTarget> : IAspectLayerBuilder, IAspectReceiverSelector<TAspectTarget>
        where TAspectTarget : class, IDeclaration
    {
        /// <summary>
        /// Gets the declaration to which the aspect was added.
        /// </summary>
        new TAspectTarget Target { get; }
    }
}