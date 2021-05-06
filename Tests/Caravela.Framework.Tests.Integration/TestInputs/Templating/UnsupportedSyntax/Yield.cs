using System.Collections.Generic;
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.Yield
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        IEnumerable<int> Template()
        {
            yield return 1;

            if (meta.Parameters.Count == 0)
            {
                yield break;
            }

            meta.Proceed();
        }
    }

    class TargetCode
    {
        IEnumerable<int> Method(int a)
        {
            yield return a;
        }
    }
}