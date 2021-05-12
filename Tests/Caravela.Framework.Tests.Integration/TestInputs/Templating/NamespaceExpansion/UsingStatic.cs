using System;

using Caravela.Framework.Aspects;
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