using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.PatternMatching.PropertyPatternScopeMismatch
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            var rt = new object();
            var a3 = rt is IParameter p3 && p3.DefaultValue.HasValue;

            return meta.Proceed();
        }
    }

    internal class TargetCode
    {
        private int Method( int a )
        {
            return a;
        }
    }
}