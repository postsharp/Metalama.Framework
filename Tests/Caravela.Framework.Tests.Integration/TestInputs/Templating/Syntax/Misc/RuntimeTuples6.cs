using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Misc.RunTimeTuples6
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            (int a, byte b) left = (5, 10);
            Console.WriteLine(left.a);

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