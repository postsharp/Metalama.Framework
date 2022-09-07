// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Fabrics;

/// <summary>
/// An implementation of <see cref="IAspectSource"/> that relies on a delegate that returns the aspect instances.
/// </summary>
internal class ProgrammaticAspectSource<TAspect, TDeclaration> : IAspectSource
    where TDeclaration : class, IDeclaration
    where TAspect : IAspect<TDeclaration>
{
    private readonly Func<CompilationModel, IDiagnosticAdder, IEnumerable<AspectInstance>> _getInstances;
    private readonly Func<CompilationModel, IDiagnosticAdder, IEnumerable<AspectRequirement>> _getRequirements;

    public ProgrammaticAspectSource(
        IAspectClass aspectClass,
        Func<CompilationModel, IDiagnosticAdder, IEnumerable<AspectInstance>>? getInstances = null,
        Func<CompilationModel, IDiagnosticAdder, IEnumerable<AspectRequirement>>? getRequirements = null )
    {
        if ( aspectClass.FullName != typeof(TAspect).FullName )
        {
            throw new ArgumentOutOfRangeException( nameof(aspectClass) );
        }

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
        => new(
            this._getInstances( compilation, diagnosticAdder ),
            requirements: this._getRequirements( compilation, diagnosticAdder ) );
}