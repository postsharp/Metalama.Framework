using System;
using Caravela.Framework.Aspects;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Aspects.Samples.SimpleLogging
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
