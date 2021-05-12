using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Project;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.LocalFuncNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            void LocalFunc(dynamic p)
            {
                Console.WriteLine(p.ToString());
            }

            dynamic result = meta.Proceed();

            LocalFunc(result);

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