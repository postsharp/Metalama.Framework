using System;

using static Caravela.Framework.Aspects.TemplateContext;
using static System.Math;
using Caravela.Framework.Project;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.TestInputs.Templating.NamespaceExpansion
{
    namespace UsingStatic
    {
        [CompileTime]
        class Aspect
        {
            [TestTemplate]
            dynamic Template()
            {
                Console.Write(PI);
                Console.Write(Max(0, 1));

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