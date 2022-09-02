// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Diagnostics;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// An implementation of <see cref="IAspectWeaver"/> that represents a missing aspect weaver. Emits an error when used.
/// </summary>
internal class ErrorAspectWeaver : IAspectWeaver
{
    private readonly AspectClass _aspectClass;

    public ErrorAspectWeaver( AspectClass aspectClass )
    {
        this._aspectClass = aspectClass;
    }

    public void Transform( AspectWeaverContext context )
    {
        context.ReportDiagnostic(
            GeneralDiagnosticDescriptors.CannotFindAspectWeaver.CreateRoslynDiagnostic( null, (this._aspectClass.WeaverType!, this._aspectClass.ShortName) ) );
    }
}