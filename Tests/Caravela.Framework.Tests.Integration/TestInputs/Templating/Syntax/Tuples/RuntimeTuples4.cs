using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.RunTimeTuples4
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var a = 1;
            var b = 2; 
            
            var namedItems = (a, b);
            Console.WriteLine(namedItems.a);
            
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