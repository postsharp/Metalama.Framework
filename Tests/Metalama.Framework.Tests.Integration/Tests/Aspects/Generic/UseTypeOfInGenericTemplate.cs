using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Generic.UseTypeOfInGenericTemplate
{
    internal class Aspect : TypeAspect
    {
        [Introduce]
        public T GenericMethod<T>( T a )
        {
            Console.WriteLine( typeof(T).Name );

            return a;
        }
    }

    // <target>
    [Aspect]
    internal class TargetCode { }
}