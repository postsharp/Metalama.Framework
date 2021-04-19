#pragma warning disable CS8600, CS8603
using Caravela.Framework.Project;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.IfTests.IfResult
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            dynamic result = proceed();

            if (result == null)
            {
                return "";
            }

            return result;
        }
    }

    class TargetCode
    {
        string Method(object a)
        {
            return a?.ToString();
        }
    }
}