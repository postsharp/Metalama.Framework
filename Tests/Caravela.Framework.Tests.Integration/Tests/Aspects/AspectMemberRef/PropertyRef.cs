using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.AspectMemberRef.PropertyRef
{

    public class RetryAttribute : OverrideMethodAspect
    {
        public int Property { get; set; } = 5;
    
        public override dynamic? OverrideMethod()
        {
           Console.WriteLine( this.Property );
           return meta.Proceed();
        }
    }
    
    class Program
    {
        [Retry(Property = 10)]
        [TestOutput]
        static int Foo()
        {
            return 0;
    
        }
    }
}