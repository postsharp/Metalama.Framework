using System;
using System.Collections.Generic;
using Caravela.Framework;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Generic.UseTypeOfInGenericTemplate
{
    class Aspect : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public T GenericMethod<T>(T a )
        {
            Console.WriteLine( typeof(T).Name );
            return a;
        }
    }

    // <target>
    [Aspect]
    class TargetCode
    {
        
    }
}