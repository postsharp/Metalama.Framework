global using Metalama.Framework.Tests.Integration.TestInputs.Templating.NamespaceExpansion.GlobalUsing.MyNamespaceGoingGlobal;

using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.TestInputs.Templating.NamespaceExpansion
{
    namespace GlobalUsing
    {
        [CompileTime]
        class Aspect
        {
            [TestTemplate]
            dynamic? Template()
            {
                MyClassGoingGlobal.MyMethod();

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

        namespace MyNamespaceGoingGlobal
        {
            class MyClassGoingGlobal
            {
                public static void MyMethod()
                {
                }
            }
        }
    }
}