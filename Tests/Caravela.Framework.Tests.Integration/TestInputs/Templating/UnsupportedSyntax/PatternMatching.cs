using System;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.PatternMatching
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            dynamic result = proceed();

            switch (result)
            {
                case string s:
                    Console.WriteLine(s);
                    break;
                case int i when i < 0:
                    throw new IndexOutOfRangeException();
                case var x:
                    break;
            }

            return result;
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