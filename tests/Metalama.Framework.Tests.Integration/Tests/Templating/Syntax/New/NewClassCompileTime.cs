using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.New.CompileTimeNewClass
{

    [CompileTime]
    class CompileTimeClass
    {
        public string String = "string";
    }
    
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var c = meta.CompileTime(new CompileTimeClass());
            Console.WriteLine(c.String);

            var c1 = new CompileTimeClass();
            Console.WriteLine(c1.String);
            
            return meta.Proceed();
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