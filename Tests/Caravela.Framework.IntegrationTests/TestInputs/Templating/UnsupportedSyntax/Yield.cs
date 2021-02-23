using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.CSharpSyntax.Yield
{
    internal class Aspect
    {
        [TestTemplate]
        private IEnumerable<int> Template()
        {
            yield return 1;

            if (target.Parameters.Count == 0)
            {
                yield break;
            }
            proceed();
        }
    }

    internal class TargetCode
    {
        private IEnumerable<int> Method(int a)
        {
            yield return a;
        }
    }
}