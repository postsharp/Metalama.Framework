using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Aspects.Diagnostics.SkipWithoutError
{
    public class SkippedAttribute : OverrideMethodAspect
    {
        public override void Initialize(IAspectBuilder<IMethod> aspectBuilder)
        {
            base.Initialize(aspectBuilder);

            aspectBuilder.SkipAspect();
        }

        public override dynamic? OverrideMethod()
        {
            throw new NotImplementedException("This code should not be emitted.");
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        [Skipped]
        public static int Add(int a, int b)
        {
            if (a == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(a));
            }

            return a + b;
        }
    }
}
