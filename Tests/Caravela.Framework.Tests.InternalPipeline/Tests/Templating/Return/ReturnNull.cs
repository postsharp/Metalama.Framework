#pragma warning disable CS8600, CS8603
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.ReturnStatements.ReturnNull
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var a = meta.Target.Parameters[0];
            var b = meta.Target.Parameters[1];
            if (a.Value == null || b.Value == null)
            {
                return null;
            }

            dynamic result = meta.Proceed();
            return result;
        }
    }

    class TargetCode
    {
        string Method(string a, string b)
        {
            return a + b;
        }
    }
}