// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using System.Collections.Immutable;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline.LiveTemplates;

/// <summary>
/// A fake instance of <see cref="IAspectSource"/> to avoid having to support null sources.
/// </summary>
internal sealed class LiveTemplateAspectSource : IAspectSource
{
    public static readonly LiveTemplateAspectSource Instance = new();

    private LiveTemplateAspectSource() { }

    public ImmutableArray<IAspectClass> AspectClasses => ImmutableArray<IAspectClass>.Empty;

    public AspectSourceResult GetAspectInstances(
        CompilationModel compilation,
        IAspectClass aspectClass,
        IDiagnosticAdder diagnosticAdder,
        CancellationToken cancellationToken )
        => AspectSourceResult.Empty;
}