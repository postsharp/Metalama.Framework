#pragma warning disable CS8600, CS8603
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.ReturnStatements.ReturnNull
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
        // <target>
        string Method(string a, string b)
        {
            return a + b;
        }
    }
}