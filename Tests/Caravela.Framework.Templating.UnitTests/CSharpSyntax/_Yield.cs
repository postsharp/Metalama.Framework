using System;
using System.Collections.Generic;

using System.Collections.Generic;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Templating.UnitTests.CSharpSyntax.Yield
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