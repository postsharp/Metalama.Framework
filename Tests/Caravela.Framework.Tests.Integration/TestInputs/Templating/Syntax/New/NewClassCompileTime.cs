using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.New.CompileTimeNewClass
{

    [CompileTimeOnly]
    class CompileTimeClass
    {
        public string String = "string";
    }
    
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var c = compileTime(new CompileTimeClass());
            Console.WriteLine(c.String);

            var c1 = new CompileTimeClass();
            Console.WriteLine(c1.String);
            
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