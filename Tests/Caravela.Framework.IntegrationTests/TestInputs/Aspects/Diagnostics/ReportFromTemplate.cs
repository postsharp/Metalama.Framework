using System;
using Caravela.Framework.Aspects;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Aspects.Diagnostics.ReportFromTemplate
{
    public class LogAttribute : OverrideMethodAspect
    {

        public static Framework.Diagnostics.DiagnosticDescriptor Error1 = new("MY001", Framework.Diagnostics.DiagnosticSeverity.Error, "Invalid method {0}.");
        public override dynamic OverrideMethod()
        {
            Error1.Report(target.Method);
            return proceed();
        }
    }

    #region Target
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
    #endregion
}
