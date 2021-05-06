using System;
using System.Text;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.New.CompileTimeNew
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var o = compileTime(new StringBuilder());
            o.Append("x");
            Console.WriteLine(o.ToString());
            
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