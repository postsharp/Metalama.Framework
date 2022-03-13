// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
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

internal readonly struct AspectSourceResult
{
    public IEnumerable<AspectInstance> AspectInstances { get; }

    public IEnumerable<Ref<IDeclaration>> Exclusions { get; }

    public static AspectSourceResult Empty => new( null );

    public AspectSourceResult( IEnumerable<AspectInstance>? aspectInstances, IEnumerable<Ref<IDeclaration>>? exclusions = null )
    {
        this.AspectInstances = aspectInstances ?? Enumerable.Empty<AspectInstance>();
        this.Exclusions = exclusions ?? Enumerable.Empty<Ref<IDeclaration>>();
    }
}