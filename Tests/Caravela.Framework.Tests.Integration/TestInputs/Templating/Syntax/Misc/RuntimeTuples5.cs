using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Misc.RunTimeTuples
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var items = (a : 1, b: 2, 3);
            Console.WriteLine(items.a);
           
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