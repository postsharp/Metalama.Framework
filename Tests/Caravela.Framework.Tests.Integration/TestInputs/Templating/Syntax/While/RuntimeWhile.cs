using System;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.While.RunTimeWhile
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            int i = 0;
            while (i < target.Parameters.Count)
            {
                i++;
            }

            Console.WriteLine("Test result = " + i);

            dynamic result = proceed();
            return result;
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