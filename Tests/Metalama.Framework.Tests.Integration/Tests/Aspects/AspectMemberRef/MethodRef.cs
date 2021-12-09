using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.AspectMemberRef.MethodRef
{

    public class RetryAttribute : OverrideMethodAspect
    {
        public string GetParameterName() => meta.Target.Parameters.First().Name;
        public static string GetParameterNameStatic(IParameter p) => p.Name;
    
        public override dynamic? OverrideMethod()
        {
           Console.WriteLine( this.GetParameterName() );
           Console.WriteLine( GetParameterNameStatic(meta.Target.Parameters.First()) );
           return meta.Proceed();
        }
    }
    
    class Program
    {
        // <target>
        [Retry]
        static int Foo(int a)
        {
            return 0;
    
        }
    }
}