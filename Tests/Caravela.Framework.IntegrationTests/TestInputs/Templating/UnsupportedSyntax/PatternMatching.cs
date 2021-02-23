using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.CSharpSyntax.PatternMatching
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
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

    internal class TargetCode
    {
        private int Method(int a)
        {
            return a;
        }
    }
}