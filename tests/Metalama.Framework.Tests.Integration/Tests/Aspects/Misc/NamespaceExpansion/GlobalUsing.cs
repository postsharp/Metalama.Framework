global using Metalama.Framework.IntegrationTests.Aspects.Misc.NamespaceExpansion.GlobalUsing.MyNamespaceGoingGlobal;

using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Misc.NamespaceExpansion.GlobalUsing
{
    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            MyClassGoingGlobal.MyMethod();
            return meta.Proceed();
        }
    }

    namespace MyNamespaceGoingGlobal
    {
        class MyClassGoingGlobal
        {
            public static void MyMethod()
            {
            }
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
