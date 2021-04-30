using System;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lambdas.CompileTimeSimpleLambda
{
    class Aspect
    {     
        [TestTemplate]
        dynamic Template()
        {
            Func<int, int> action = compileTime(new  Func<int, int> (x => x + 1));
        
            var result = compileTime(1);

            result = action(result);

            return result;
        }
    }

    class TargetCode
    {
        int Method(int a, int b)
        {
            return a + b;
        }
    }
}