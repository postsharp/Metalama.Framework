using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Aspects.Diagnostics.InvalidCompileTimeUserCode
{
    public class ErrorAttribute : OverrideMethodAspect
    {
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
#if TESTRUNNER // Avoid the code to be parsed in the IDE.
            aspectBuilder.BadMethod();
#endif
        }

        public override dynamic OverrideMethod()
        {
            throw new NotImplementedException("This code should not be emitted.");
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        [Error]
        public static int Add(int a, int b)
        {
            return a + b;
        }
    }
}
