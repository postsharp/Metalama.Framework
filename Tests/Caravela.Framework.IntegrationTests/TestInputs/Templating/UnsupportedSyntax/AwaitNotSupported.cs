using System.Threading.Tasks;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.UnsupportedSyntax.AwaitNotSupported
{
    internal class Aspect
    {
        [TestTemplate]
        private async Task<T> Template<T>()
        {
            await Task.Yield();

            dynamic result = proceed();
            return result;
        }
    }

    internal class TargetCode
    {
        private async Task<int> Method(int a, int b)
        {
            return await Task.FromResult(a + b);
        }
    }
}