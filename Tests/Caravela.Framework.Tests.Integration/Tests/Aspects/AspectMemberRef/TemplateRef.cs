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
        [Template]
        void Template() {}
    
        public override dynamic OverrideMethod()
        {
            this.Template();
           return meta.Proceed();
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