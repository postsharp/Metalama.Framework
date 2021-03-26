using System;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;
using MyMath = System.Math;

namespace Caravela.Framework.Tests.Integration.TestInputs.Templating.Alias
{
    namespace UsingStatic
    {
        class Aspect
        {
            [TestTemplate]
            dynamic Template()
            {
                Console.Write(MyMath.PI);
                
                return proceed();
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