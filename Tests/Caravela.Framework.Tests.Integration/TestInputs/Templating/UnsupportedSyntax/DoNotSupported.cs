using System;
using static Caravela.Framework.Aspects.TemplateContext;
using Caravela.Framework.Project;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.DoNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int i = 0;
            do
            {
                i++;
            } while (i < target.Parameters.Count);

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