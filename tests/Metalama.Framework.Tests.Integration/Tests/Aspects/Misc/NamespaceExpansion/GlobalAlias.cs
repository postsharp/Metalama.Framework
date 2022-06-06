global using MyGlobalMath = System.Math;

using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Misc.NamespaceExpansion.GlobalAlias
{
    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine(MyGlobalMath.PI);
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
