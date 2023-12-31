// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System.Collections.Immutable;
using System.Threading;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// Defines the semantics of an object that can return a set of <see cref="AspectInstance"/>
/// for a given <see cref="IAspectClass"/>.
/// </summary>
internal interface IAspectSource
{
    ImmutableArray<IAspectClass> AspectClasses { get; }

    /// <summary>
    /// Returns a set of <see cref="AspectInstance"/> of a given type. This method is called when the given aspect
    /// type is being processed, not before.
    /// </summary>
    AspectSourceResult GetAspectInstances(
        CompilationModel compilation,
        IAspectClass aspectClass,
        IDiagnosticAdder diagnosticAdder,
        CancellationToken cancellationToken );
}