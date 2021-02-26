using System;
using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.LocalVariables.CompileTimeDeclaratorInCompileTimeBlock
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            if (target.Parameters.Count > 0)
            {
                var x = compileTime(0);
                Console.WriteLine(x);
            }

            if (target.Parameters.Count > 1)
            {
                var x = compileTime(1);
                Console.WriteLine(x);
            }

            foreach (var p in target.Parameters)
            {
                var y = compileTime(0);
                Console.WriteLine(y);
            }

            return proceed();
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