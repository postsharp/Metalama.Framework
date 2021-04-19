using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.New.NewRunTimeClassInCompileTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var o = compileTime(new TargetCode());
            Console.WriteLine(o.GetType().ToString());

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
