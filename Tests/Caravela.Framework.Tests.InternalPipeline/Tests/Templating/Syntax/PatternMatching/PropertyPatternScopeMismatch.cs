using Caravela.Framework.Code;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.PatternMatching.PropertyPatternScopeMismatch
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