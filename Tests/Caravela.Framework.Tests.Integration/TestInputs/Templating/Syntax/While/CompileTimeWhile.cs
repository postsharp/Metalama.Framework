using System;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.While.CompileTimeWhile
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var i = compileTime(0);
            while (i < target.Method.Name.Length)
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