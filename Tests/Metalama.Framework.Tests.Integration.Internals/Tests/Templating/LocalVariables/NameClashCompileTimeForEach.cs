using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.LocalVariables.NameClashCompileTimeForEach
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            foreach (var p in meta.Target.Parameters)
            {
                string text = p.Name + " = " + p.Value;
                Console.WriteLine(text);
            }

            return meta.Proceed();
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