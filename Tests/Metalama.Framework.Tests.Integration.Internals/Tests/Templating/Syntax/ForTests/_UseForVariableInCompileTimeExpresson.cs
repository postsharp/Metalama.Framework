// @Skipped

using System;
using Metalama.Framework.Aspects;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.ForTests.UseForVariableInCompileTimeExpresson
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            for (int i = 0; i < meta.Target.Parameters.Count; i++)
            {
                Console.WriteLine(meta.Target.Parameters[i].Name);
            }

            dynamic? result = meta.Proceed();
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