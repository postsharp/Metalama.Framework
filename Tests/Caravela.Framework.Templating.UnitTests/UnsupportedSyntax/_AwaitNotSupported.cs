using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Threading.Tasks;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Templating.UnitTests.UnsupportedSyntax.AwaitNotSupported
{
    class Aspect
    {
        [TestTemplate]
        async Task<T> Template<T>()
        {
            await Task.Yield();

            dynamic result = proceed();
            return result;
        }
    }

    class TargetCode
    {
        async Task<int> Method(int a, int b)
        {
            return await Task.FromResult(a + b);
        }
    }
}