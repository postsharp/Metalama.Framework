#if TEST_OPTIONS
// @TestScenario(CodeFix)
#endif

using System.IO;
using System.Linq;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.CodeFixes.ProjectValidatorWithFix;

internal class MyProjectFabric : ProjectFabric
{
    private static DiagnosticDefinition<IField> _warning = new( "MY001", Severity.Warning, "The field {0} must be private." );

    public override void AmendProject( IProjectAmender amender )
    {
        amender.SelectMany(
                p => p.Types.SelectMany( t => t.Fields.Where( f => f.Accessibility != Accessibility.Private && f.Type.Is( typeof(TextWriter) ) ) ) )
            .ReportDiagnostic( f => _warning.WithArguments( f ).WithCodeFixes( CodeFixFactory.ChangeAccessibility( f, Accessibility.Private ) ) );
    }
}

// <target>
internal class SomeType
{
    public TextWriter? Writer;
}