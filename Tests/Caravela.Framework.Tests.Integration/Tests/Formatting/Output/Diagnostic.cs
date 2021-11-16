// @Skipped(29354)
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Tests.Integration.Tests.Formatting.Output.Diagnostic
{
    public class AddWarning : MethodAspect
    {
        // We don't test errors because the pipeline would not succeed. Errors are not different than warnings anyway.
        private static DiagnosticDefinition _warning = new( "MY001", Severity.Warning, "Test, including special characters: <>&\"\n\r" );

        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Diagnostics.Report( _warning );
        }
    }

    public class TargetCode
    {
        [AddWarning]
        public void Method1() { }
    }
}