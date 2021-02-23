using System;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.LocalVariables.SimpleAssignments
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            var n = target.Parameters.Count; // build-time
                                             //var n = runTime(target.Method.Parameters.Count); // run-time
            var a0 = target.Parameters[0].Value; // run-time
            var x = 0; // run-time
            var y = compileTime(0); // compile-time    

            Console.WriteLine(n);
            Console.WriteLine(a0);
            Console.WriteLine(x);
            Console.WriteLine(y);

            return proceed();
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