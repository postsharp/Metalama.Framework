// @Skipped(29354)
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.Output.Diagnostic
{
    public class AddWarning : MethodAspect
    {
        // We don't test errors because the pipeline would not succeed. Errors are not different than warnings anyway.
        private static DiagnosticDefinition _warning = new( "MY001", Severity.Warning, "Test, including special characters: <>&\"\n\r" );

        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            _warning.ReportTo( builder.Diagnostics );
        }
    }

    public class TargetCode
    {
        [AddWarning]
        public void Method1() { }
    }
}