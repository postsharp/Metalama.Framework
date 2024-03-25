using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Generic.AccessGenericParametersFromTemplate
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            foreach ( var genericParameter in meta.Target.Method.TypeParameters )
            {
                var v = genericParameter.DefaultValue();
                var t = meta.RunTime( genericParameter.ToType() );
            }
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