using System;
using static Caravela.Framework.Aspects.TemplateContext;
using Caravela.Framework.Project;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.UnsafeNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int i = target.Parameters.Count;
            unsafe
            {
                int* p = &i;

                *p = 42;
            }

            Console.WriteLine("Test result = " + i);

            dynamic result = proceed();
            return result;
        }
    }

    class TargetCode
    {
        int Method(int a, int b)
        {
            return a + b;
        }
    }
}