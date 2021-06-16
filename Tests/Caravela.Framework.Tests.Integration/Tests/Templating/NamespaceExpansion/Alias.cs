using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;
using MyMath = System.Math;

namespace Caravela.Framework.Tests.Integration.TestInputs.Templating.Alias
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