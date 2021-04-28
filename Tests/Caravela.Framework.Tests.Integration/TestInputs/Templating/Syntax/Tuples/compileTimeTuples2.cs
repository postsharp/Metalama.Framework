using System;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.CompileTimeTuples2
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var t = compileTime((1, 2, 3));
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