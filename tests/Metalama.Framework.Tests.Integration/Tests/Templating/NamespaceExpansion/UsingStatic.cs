using System;

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;
using static System.Math;

namespace Metalama.Framework.Tests.Integration.TestInputs.Templating.NamespaceExpansion
{
    namespace UsingStatic
    {
        [CompileTime]
        class Aspect
        {
            [TestTemplate]
            dynamic? Template()
            {
                Console.Write(PI);
                Console.Write(Max(0, 1));

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