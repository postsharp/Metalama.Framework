using Metalama.Framework.Engine.AspectWeavers;
using Metalama.Framework.Engine.Diagnostics;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// An implementation of <see cref="IAspectWeaver"/> that represents a missing aspect weaver. Emits an error when used.
/// </summary>
internal class ErrorAspectWeaver : IAspectWeaver
{
    private AspectClass _aspectClass;

    public ErrorAspectWeaver( AspectClass aspectClass ) {
        this._aspectClass = aspectClass;
    }

    public void Transform( AspectWeaverContext context )
    {
        context.ReportDiagnostic( GeneralDiagnosticDescriptors.CannotFindAspectWeaver.CreateRoslynDiagnostic( null, (this._aspectClass.WeaverType, this._aspectClass.ShortName) ) );

    }
}