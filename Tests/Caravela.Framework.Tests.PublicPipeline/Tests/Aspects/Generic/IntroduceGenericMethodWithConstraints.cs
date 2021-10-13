using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Generic.IntroduceGenericMethodWithConstraints
{
    internal class Aspect : TypeAspect
    {
        [Introduce]
        public T GenericMethod<T>( T a )
            where T : notnull, IDisposable, new()
        {
            return a;
        }
    }

    // <target>
    [Aspect]
    internal class TargetCode { }
}