using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;
using static System.Math;

namespace Metalama.Framework.IntegrationTests.Aspects.Misc.NamespaceExpansion.UsingStatic
{
    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine(PI);
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
