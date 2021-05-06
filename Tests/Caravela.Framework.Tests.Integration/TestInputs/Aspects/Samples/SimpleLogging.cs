using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using meta = Caravela.Framework.Aspects.meta;

namespace Caravela.Framework.Tests.Integration.Aspects.Samples.SimpleLogging
{
    public class LogAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine(meta.Method.ToDisplayString() + " started.");

            try
            {
                dynamic result = meta.Proceed();

                Console.WriteLine(meta.Method.ToDisplayString() + " succeeded.");
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(meta.Method.ToDisplayString() + " failed: " + e.Message);
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
