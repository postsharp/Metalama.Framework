using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Generic.OverrideGenericMethodWithTwoGenericParams
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            return meta.Proceed();
        }
    }

    // <target>
    class TargetCode
    {
        [Aspect]
        T Method<T,S>(T a, S b)
        {
            return a;
        }
    }
}