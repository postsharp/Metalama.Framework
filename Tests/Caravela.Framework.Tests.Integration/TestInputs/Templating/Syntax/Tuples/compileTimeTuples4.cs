using System;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.CompileTimeTuples4
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var a = compileTime(1);
            var b = compileTime(2); 
            
            var namedItems = compileTime((a, b));
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