using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Diagnostics;
using Caravela.TestFramework;


namespace Caravela.Framework.Tests.Integration.Aspects.Diagnostics.ReportFromTemplate
{
    public class LogAttribute : OverrideMethodAspect
    {
        private static readonly DiagnosticDefinition _definition = new( "MY001", Severity.Error, "Invalid method." );
        
        public override dynamic? OverrideMethod()
        {
            meta.Diagnostics.Report( _definition );

            return meta.Proceed();
        }
    }

    [TestOutput]
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
