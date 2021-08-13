using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Cast.CompileTimeCast
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            short c = (short)meta.Target.Parameters.Count;

            if (c > 0)
            {
                string text = meta.CompileTime("");
                object s = meta.Target.Parameters[0].Name;
                if (s is string)
                {
                    text = (s as string) + " = ";
                }

                Console.WriteLine(text + meta.Target.Parameters[0].Value);
            }

            return meta.Proceed();
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