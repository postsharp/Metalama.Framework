global using MyGlobalMath = System.Math;

using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.TestInputs.Templating.NamespaceExpansion
{
    namespace GlobalAlias
    {
        [CompileTime]
        class Aspect
        {
            [TestTemplate]
            dynamic? Template()
            {
                Console.Write(MyGlobalMath.PI);

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
    }
}