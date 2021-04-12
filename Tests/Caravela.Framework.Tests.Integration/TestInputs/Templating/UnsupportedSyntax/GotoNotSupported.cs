#pragma warning disable CS8600, CS8603
using static Caravela.Framework.Aspects.TemplateContext;
using Caravela.Framework.Project;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.GotoNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            dynamic result = proceed();

            if (result != null) goto end;

            return default;

        end:
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