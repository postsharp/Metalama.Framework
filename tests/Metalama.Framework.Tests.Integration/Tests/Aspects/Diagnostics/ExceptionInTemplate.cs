using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Aspects.Diagnostics.ExceptionInTemplate
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var a = meta.CompileTime(0);
            var b = 1 / a;
            return meta.Proceed();
        }
                
    }

    class TargetCode
    {
        [Aspect]
        int Method(int a)
        {
            return a;
        }
    }
}