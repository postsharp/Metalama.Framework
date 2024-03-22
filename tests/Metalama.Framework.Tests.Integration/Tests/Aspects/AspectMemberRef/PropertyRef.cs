using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.AspectMemberRef.PropertyRef
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
        // <target>
        [Retry(Property = 10)]
        static int Foo()
        {
            return 0;
    
        }
    }
}