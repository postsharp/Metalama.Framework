using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.CompileTimeTuples
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var items = compileTime((a : 1, b: 2, 3));
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