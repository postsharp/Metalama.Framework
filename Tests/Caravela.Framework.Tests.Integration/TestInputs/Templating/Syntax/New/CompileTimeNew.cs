using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.New.CompileTimeNew
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var o = compileTime(new object());
            Console.WriteLine(o.GetType().ToString());
            
            var t = compileTime(new TargetCode());
            
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