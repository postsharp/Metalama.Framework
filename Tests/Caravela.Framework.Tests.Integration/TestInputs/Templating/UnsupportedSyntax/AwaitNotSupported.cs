using System.Threading.Tasks;
using Caravela.Framework.Project;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.AwaitNotSupported
{
    [CompileTime]
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