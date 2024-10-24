using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;
using System;

namespace Metalama.Framework.Tests.AspectTests.Tests.Fabrics.AttributeDiagnostic;

internal class Fabric : ProjectFabric
{
    private static DiagnosticDefinition error = new( "TEST01", Severity.Error, "ErrorAttribute was used." );

    public override void AmendProject( IProjectAmender amender )
    {
        amender
            .SelectMany( compilation => compilation.AllTypes )
            .SelectMany( type => type.Attributes )
            .Where( attribute => attribute.Type.IsConvertibleTo( typeof(ErrorAttribute) ) )
            .ReportDiagnostic( _ => error );
    }
}

[RunTimeOrCompileTime]
public class ErrorAttribute : Attribute { }

[Error]
internal class C { }