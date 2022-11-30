#pragma warning disable CS8600, CS8603
using Metalama.Testing.Framework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.IfTests.IfResult
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            dynamic result = meta.Proceed();

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