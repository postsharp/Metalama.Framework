using System;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Methods.OutParam
{
    // Tests single OverrideMethod aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            Console.WriteLine("This is the overriding method.");
            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public void TargetMethod_OutParam(out int a)
        {
            a = 0;
        }
        
        [Override]
        public void TargetMethod_RefParam(ref int a)
        {
            a = 0;
        }
        
        [Override]
        public void TargetMethod_InParam(in DateTime a)
        {
        }
    }
}
