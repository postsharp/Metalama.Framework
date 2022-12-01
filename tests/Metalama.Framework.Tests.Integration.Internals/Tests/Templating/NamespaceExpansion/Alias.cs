using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;
using MyMath = System.Math;

namespace Metalama.Framework.Tests.Integration.TestInputs.Templating.Alias
{
    namespace UsingStatic
    {
        [CompileTime]
        class Aspect
        {
            [TestTemplate]
            dynamic? Template()
            {
                Console.Write(MyMath.PI);

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