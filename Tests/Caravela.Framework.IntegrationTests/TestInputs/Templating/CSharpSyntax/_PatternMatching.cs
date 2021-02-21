using System;
using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Templating.UnitTests.CSharpSyntax.PatternMatching
{
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