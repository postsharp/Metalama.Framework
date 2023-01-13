#pragma warning disable CS8600, CS8603
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.ReturnStatements.ReturnDefault
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
        // <target>
        int Method(int a)
        {
            return 42 / a;
        }
    }
}