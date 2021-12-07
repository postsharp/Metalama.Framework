using Metalama.Framework.Code;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.PatternMatching.PropertyPatternScopeMismatch
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var rt = new object();
            var a3 = rt is IParameter p3 && p3.DefaultValue.IsNull;
                    
            return meta.Proceed();
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}