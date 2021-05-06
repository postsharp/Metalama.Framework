#pragma warning disable CS8600, CS8603
using Caravela.Framework.Project;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.ReturnStatements.ReturnDefault
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            try
            {
                dynamic result = meta.Proceed();
                return result;
            }
            catch
            {
                return default;
            }
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return 42 / a;
        }
    }
}