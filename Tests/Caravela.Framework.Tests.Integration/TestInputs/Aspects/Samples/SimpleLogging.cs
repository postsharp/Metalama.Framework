using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Aspects.Samples.SimpleLogging
{
    public class LogAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine(target.Method.ToDisplayString() + " started.");

            try
            {
                dynamic result = proceed();

                Console.WriteLine(target.Method.ToDisplayString() + " succeeded.");
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(target.Method.ToDisplayString() + " failed: " + e.Message);
                throw;
            }
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
