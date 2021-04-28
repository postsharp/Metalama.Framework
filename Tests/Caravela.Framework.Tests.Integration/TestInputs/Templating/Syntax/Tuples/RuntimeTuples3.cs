using System;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.RunTimeTuples3
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            (int, string) anonymT = (4, "");
            Console.WriteLine(anonymT.Item1);
            
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