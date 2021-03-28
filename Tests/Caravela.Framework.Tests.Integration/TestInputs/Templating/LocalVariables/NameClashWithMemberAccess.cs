using System;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.LocalVariables.NameClashWithMemberAccess
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var n = target.Parameters.Count; // build-time

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

            return proceed();
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