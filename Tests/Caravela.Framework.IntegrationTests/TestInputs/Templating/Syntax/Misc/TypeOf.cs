#pragma warning disable CS8600, CS8603
using System;
using Caravela.Framework.Code;
using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.CSharpSyntax.Misc.TypeOf
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
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

    [CompileTime]
    public class MyClass1 { }

    internal class TargetCode
    {
        private string Method(string a)
        {
            return a;
        }
    }
}