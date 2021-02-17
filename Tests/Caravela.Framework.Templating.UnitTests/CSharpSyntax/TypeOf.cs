using System;
using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Templating.UnitTests.CSharpSyntax.TypeOf
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            string s = compileTime(typeof(string).FullName);
            Console.WriteLine(s);

            if (target.Parameters[0].Type.Is(typeof(string)))
            {
                Console.WriteLine(typeof(string).FullName);
            }

            Console.WriteLine(typeof(MyClass1).FullName);

            return proceed();
        }
    }


    [CompileTime]
    public class MyClass1 { }

    class TargetCode
    {
        string Method(string a)
        {
            return a;
        }
    }
}