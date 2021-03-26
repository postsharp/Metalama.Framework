using System;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;
using static System.Math;
namespace Caravela.Framework.Tests.Integration.TestInputs.Templating.NamespaceExpansion
{
    namespace UsingStatic
    {
        class Aspect
        {
            [TestTemplate]
            dynamic Template()
            {
                Console.Write(PI);
                
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