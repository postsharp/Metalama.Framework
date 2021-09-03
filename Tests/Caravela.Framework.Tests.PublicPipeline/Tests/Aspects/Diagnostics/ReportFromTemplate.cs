using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Diagnostics;
using Caravela.TestFramework;


namespace Caravela.Framework.Tests.Integration.Aspects.Diagnostics.ReportFromTemplate
{
    public class LogAttribute : OverrideMethodAspect
    {
        // Without argument.
        private static readonly DiagnosticDefinition _definition1 = new( "MY001", Severity.Error, "Invalid method." );
        private static readonly DiagnosticDefinition _definition2 = new( "MY002", Severity.Error, "Invalid type." );
        
        // With arguments.
        private static readonly DiagnosticDefinition<string> _definition3 = new( "MY003", Severity.Error, "Invalid method: {0}." );
        private static readonly DiagnosticDefinition<string> _definition4 = new( "MY004", Severity.Error, "Invalid type: {0}." );

        public override dynamic? OverrideMethod()
        {
            // Report on the default scope.
            meta.Diagnostics.Report( _definition1 );
            meta.Diagnostics.Report( _definition3, "Test" );
            
            // Report by specifying the scope explicitly.
            meta.Diagnostics.Report( meta.Target.Type, _definition2 );
            meta.Diagnostics.Report( meta.Target.Type, _definition4, "Test" );

            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        [Log]
        public static int Add(int a, int b)
        {
            if (a == 0)
                throw new ArgumentOutOfRangeException(nameof(a));
            return a + b;
        }
    }
}
