global using static Metalama.Framework.Tests.Integration.TestInputs.Templating.NamespaceExpansion.GlobalUsingStatic.MyClassWithStaticMethods;

using System;

using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.TestInputs.Templating.NamespaceExpansion
{
    namespace GlobalUsingStatic
    {
        [CompileTime]
        class Aspect
        {
            [TestTemplate]
            dynamic? Template()
            {
                MyMethodGoingGlobal();

                return meta.Proceed();
            }
        }

        class TargetCode
        {
            int Method(int a)
            {
                return a;
            }
        }

        class MyClassWithStaticMethods
        {
            public static void MyMethodGoingGlobal()
            {
            }
        }
    }
}