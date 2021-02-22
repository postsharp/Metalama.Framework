using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.Diagnostics.ReportFromInitialize
{
    public class ErrorAttribute : OverrideMethodAspect
    {
        public override void Initialize(IAspectBuilder<IMethod> aspectBuilder)
        {
            aspectBuilder.ReportDiagnostic(Caravela.Framework.Diagnostics.Severity.Error, "MY001", "Error");
            aspectBuilder.ReportDiagnostic(Caravela.Framework.Diagnostics.Severity.Warning, "MY002", "Warning");
            aspectBuilder.ReportDiagnostic(Caravela.Framework.Diagnostics.Severity.Info, "MY003", "Info");
            aspectBuilder.ReportDiagnostic(Caravela.Framework.Diagnostics.Severity.Hidden, "MY004", "Hidden");
        }

        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException("This code should not be emitted.");
        }
    }

    #region Target
    internal class TargetClass
    {
        [Error]
        public static int Add(int a, int b)
        {
            if (a == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(a));
            }

            return a + b;
        }
    }
    #endregion
}
