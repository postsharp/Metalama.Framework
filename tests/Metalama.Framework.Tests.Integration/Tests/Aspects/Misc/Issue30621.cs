using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue30621
{
    public class ClassA
    {
        [Log]
        public void Foo()
        {
            Console.WriteLine( "Bar" );
        }
    }

    public class ClassB : ClassA
    {
    }

    public class LogAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( "Simple log" );
            return meta.Proceed();
        }
    }
}
