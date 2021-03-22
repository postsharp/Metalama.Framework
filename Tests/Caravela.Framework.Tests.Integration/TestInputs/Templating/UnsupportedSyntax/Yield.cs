using System.Collections.Generic;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.CSharpSyntax.Yield
{
    class Aspect
    {
        [TestTemplate]
        IEnumerable<int> Template()
        {
            yield return 1;

            if (target.Parameters.Count == 0)
            {
                yield break;
            }
            proceed();
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