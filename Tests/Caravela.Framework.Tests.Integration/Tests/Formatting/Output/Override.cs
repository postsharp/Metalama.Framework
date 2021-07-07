using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Tests.Formatting.Override
{
    public class OverrideAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Generated code.");

            try
            {
                return meta.Proceed();
            }
            catch (Exception)
            {
                Console.WriteLine("Oops!");

                throw;
            }
        }
    }

    
    public class TargetCode
    {
        [OverrideAspect]
        public int Method()
        {
            Console.WriteLine("User code.");

            return 1;
        }
        
    }
}