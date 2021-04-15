using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.RunTimeTuples2
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var t = (1, 2, 3);
            Console.WriteLine(t.Item3);
            
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