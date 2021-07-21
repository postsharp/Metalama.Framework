using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Tests.Formatting.OverrideNoInlining
{
    public class MyOverrideMethod : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Generated code.");

            try
            {
                meta.Proceed();
                return meta.Proceed();
            }
            catch (Exception)
            {
                Console.WriteLine("Oops!");

                throw;
            }
        }
    }

    public class MyOverrideProperty : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get
            {
                _ = meta.Proceed();

                return meta.Proceed();
            }
            set
            {
                meta.Proceed();
                meta.Proceed();
            }
        }
    }

    
    public class TargetCode
    {
        [MyOverrideMethod]
        public int Method()
        {
            Console.WriteLine("User code.");

            return 1;
        }
        
        [MyOverrideProperty]
        public int Property { get; set; }
        
        
    }
}