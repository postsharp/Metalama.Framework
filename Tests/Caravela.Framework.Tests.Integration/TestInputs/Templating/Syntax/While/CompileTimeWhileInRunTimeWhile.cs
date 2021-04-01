using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.While.CompileTimeWhileInRunTimeWhile
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
                int j = compileTime(0);
                while (j < 2)
                {
                    i++;
                    j++;
                }
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