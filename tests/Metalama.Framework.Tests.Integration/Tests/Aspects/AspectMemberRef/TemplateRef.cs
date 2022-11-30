using Metalama.Framework.Aspects;
using Metalama.Testing.Framework;

namespace Metalama.Framework.IntegrationTests.Aspects.AspectMemberRef.TemplateRef
{

    public class RetryAttribute : OverrideMethodAspect
    {
        [Template]
        void Template() {}
    
        public override dynamic? OverrideMethod()
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