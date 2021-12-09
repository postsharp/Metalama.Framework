using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Generic.ProgrammaticOverrideGenericTemplateConstraintMismatch
{
    internal class Aspect : MethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Advices.OverrideMethod( builder.Target, nameof(OverrideMethod) );
        }

        [Template]
        public T OverrideMethod<T>( T a )
            where T : struct
        {
            Console.WriteLine( a );

            return meta.Proceed();
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