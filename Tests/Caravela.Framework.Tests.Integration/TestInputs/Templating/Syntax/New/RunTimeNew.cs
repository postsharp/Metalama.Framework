using System;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.New.RunTimeNew
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var o = new object();
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