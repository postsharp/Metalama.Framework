using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Fabrics.AttributeDiagnostic;

class Fabric : ProjectFabric
{
    static DiagnosticDefinition error = new("TEST01", Severity.Error, "ErrorAttribute was used.");

    public override void AmendProject(IProjectAmender amender)
    {
        amender
            .SelectMany(compilation => compilation.AllTypes)
            .SelectMany(type => type.Attributes)
            .Where(attribute => attribute.Type.Is(typeof(ErrorAttribute)))
            .ReportDiagnostic(_ => error);
    }
}

[RunTimeOrCompileTime]
public class ErrorAttribute : Attribute
{
}

[Error]
class C { }