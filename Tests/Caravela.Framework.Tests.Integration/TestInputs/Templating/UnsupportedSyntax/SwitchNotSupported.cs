#pragma warning disable CS8600, CS8603
using static Caravela.Framework.Aspects.TemplateContext;
using Caravela.Framework.Project;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.SwitchNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            dynamic result;
            switch (target.Parameters.Count)
            {
                case 0:
                    result = null;
                    break;
                case 1:
                    result = target.Parameters[0].Value;
                    break;
                case 2:
                    goto default;
                case 3:
                    goto case 2;
                default:
                    result = proceed();
                    break;
            }

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