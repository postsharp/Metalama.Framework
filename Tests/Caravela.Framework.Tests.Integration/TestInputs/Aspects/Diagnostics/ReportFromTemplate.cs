using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using meta = Caravela.Framework.Aspects.meta;

namespace Caravela.Framework.Tests.Integration.Aspects.Diagnostics.ReportFromTemplate
{
    public class LogAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            meta.Diagnostics.Report(Caravela.Framework.Diagnostics.Severity.Error, "MY001", "Invalid method.");

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
