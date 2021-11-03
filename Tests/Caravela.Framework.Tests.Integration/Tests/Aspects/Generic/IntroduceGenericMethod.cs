using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Generic.IntroduceGenericMethod
{
    internal class Aspect : TypeAspect
    {
        [Introduce]
        public T GenericMethod<T>( T a )
        {
            return a;
        }
    }

    // <target>
    [Aspect]
    internal class TargetCode { }
}