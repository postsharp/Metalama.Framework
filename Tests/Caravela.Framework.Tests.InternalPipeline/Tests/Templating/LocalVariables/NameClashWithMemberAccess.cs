using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.LocalVariables.NameClashWithMemberAccess
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var n = meta.Target.Parameters.Count; // build-time

            if (n == 1)
            {
                var WriteLine = 0;
                Console.WriteLine(WriteLine);
            }

            if (n == 1)
            {
                var WriteLine = 1;
                Console.WriteLine(WriteLine);
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