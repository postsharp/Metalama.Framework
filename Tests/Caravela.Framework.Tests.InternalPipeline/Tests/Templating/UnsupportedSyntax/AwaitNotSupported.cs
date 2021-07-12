using System.Threading.Tasks;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.AwaitNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        async Task<T> Template<T>()
        {
            await Task.Yield();

            dynamic result = meta.Proceed();
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