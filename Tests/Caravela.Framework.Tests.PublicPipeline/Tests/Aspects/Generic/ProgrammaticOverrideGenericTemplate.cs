using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Generic.ProgrammaticOverrideGenericTemplate
{
    internal class Aspect : Attribute, IAspect<IMethod>
    {
        public void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Advices.OverrideMethod( builder.Target, nameof(OverrideMethod) );
        }

        [Template]
        public T OverrideMethod<T>( T a )
        {
            Console.WriteLine( a );

            return meta.Proceed()!;
        }
    }

    // <target>
    internal class TargetCode
    {
        [Aspect]
        private T Method<T>( T a )
        {
            return a;
        }
    }
}