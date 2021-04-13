#pragma warning disable CS8600, CS8603
using System;
using Caravela.Framework.Code;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.Misc.TypeOf
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            string s = compileTime(typeof(string).FullName);
            Console.WriteLine(s);

            if (target.Parameters[0].ParameterType.Is(typeof(string)))
            {
                Console.WriteLine(typeof(string).FullName);
            }

            Console.WriteLine(typeof(MyClass1).FullName);

            return proceed();
        }
    }

    [CompileTimeOnly]
    public class MyClass1 { }

    class TargetCode
    {
        string Method(string a)
        {
            return a;
        }
    }
}