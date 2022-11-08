// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Fabrics;

internal class ProgrammaticAspectSource : IAspectSource
{
    private readonly Func<CompilationModel, IDiagnosticAdder, IEnumerable<AspectInstance>> _getInstances;
    private readonly Func<CompilationModel, IDiagnosticAdder, IEnumerable<AspectRequirement>> _getRequirements;

    public Type AspectType { get; }

    public ProgrammaticAspectSource(
        Type aspectType,
        IAspectClass aspectClass,
        Func<CompilationModel, IDiagnosticAdder, IEnumerable<AspectInstance>>? getInstances = null,
        Func<CompilationModel, IDiagnosticAdder, IEnumerable<AspectRequirement>>? getRequirements = null )
    {
        if ( aspectClass.FullName != aspectType.FullName )
        {
            throw new ArgumentOutOfRangeException( nameof(aspectClass) );
        }

        this.AspectType = aspectType;

        this._getInstances = getInstances ?? (( _, _ ) => Enumerable.Empty<AspectInstance>());
        this._getRequirements = getRequirements ?? (( _, _ ) => Enumerable.Empty<AspectRequirement>());
        this.AspectClasses = ImmutableArray.Create( aspectClass );
    }

    public ImmutableArray<IAspectClass> AspectClasses { get; }

    public AspectSourceResult GetAspectInstances(
        CompilationModel compilation,
        IAspectClass aspectClass,
        IDiagnosticAdder diagnosticAdder,
        CancellationToken cancellationToken )
    {
        var aspectInstances = this._getInstances( compilation, diagnosticAdder );

        var requirements = this._getRequirements( compilation, diagnosticAdder );

        return new AspectSourceResult( aspectInstances, requirements: requirements );
    }
}