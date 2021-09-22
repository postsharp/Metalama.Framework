using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Generic.AccessParametersOfGenericType
{
    class Aspect : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            foreach ( var parameter in meta.Target.Parameters )
            {
                var v = parameter.Value;
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