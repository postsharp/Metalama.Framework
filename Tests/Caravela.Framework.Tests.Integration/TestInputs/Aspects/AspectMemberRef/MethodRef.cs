using System;
using System.Text;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.AspectMemberRef.MethodRef
{

    public class RetryAttribute : OverrideMethodAspect
    {
        public string GetParameterName() => meta.Parameters.First().Name;
        public static string GetParameterNameStatic(IParameter p) => p.Name;
    
        public override dynamic OverrideMethod()
        {
           Console.WriteLine( this.GetParameterName() );
           Console.WriteLine( GetParameterNameStatic(meta.Parameters.First()) );
           return default;
        }
    }
    
    class Program
    {
        [Retry]
        [TestOutput]
        static int Foo(int a)
        {
            return 0;
    
        }
    }
}