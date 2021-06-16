using Caravela.Framework.Aspects;
using Caravela.TestFramework;

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
    
    // <target>
    class Program
    {
        [Retry]
        static int Foo(int a)
        {
            return 0;
    
        }
    }
}