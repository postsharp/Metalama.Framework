using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;
using MyMath = System.Math;

namespace Metalama.Framework.IntegrationTests.Aspects.Misc.NamespaceExpansion.Alias
{
    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine(MyMath.PI);
            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void()
        {
            Console.WriteLine("This is the original method.");
        }
    }
}
