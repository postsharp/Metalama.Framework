global using static Metalama.Framework.IntegrationTests.Aspects.Misc.NamespaceExpansion.GlobalUsingStatic.MyClassWithStaticMethods;

using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Misc.NamespaceExpansion.GlobalUsingStatic
{
    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            MyMethodGoingGlobal();
            return meta.Proceed();
        }
    }

    class MyClassWithStaticMethods
    {
        public static void MyMethodGoingGlobal()
        {
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
