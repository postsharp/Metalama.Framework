using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Generic.IntroduceGenericMethodWithConstraints
{
    class Aspect : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public T GenericMethod<T>(T a )
            where T : notnull, IDisposable, new()
        {
            return a;
        }
    }

    // <target>
    [Aspect]
    class TargetCode
    {
        
    }
}