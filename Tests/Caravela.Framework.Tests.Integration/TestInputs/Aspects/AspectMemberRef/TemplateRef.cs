using System;
using System.Text;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.AspectMemberRef.TemplateRef
{

    public class RetryAttribute : OverrideMethodAspect
    {
        [Introduce]
        void Introduced() {}
    
        public override dynamic OverrideMethod()
        {
            this.Introduced();
           return default;
        }
    }
    
    [TestOutput]
    class Program
    {
        [Retry]
        static int Foo(int a)
        {
            return 0;
    
        }
    }
}