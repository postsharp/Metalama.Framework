using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.CompileTimeTuples1
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            Tuple<string, int> tuple = compileTime(new Tuple<string, int>("string", 0));
            Console.WriteLine(tuple.Item1);
            
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