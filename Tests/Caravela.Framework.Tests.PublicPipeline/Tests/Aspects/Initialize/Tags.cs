using System;
using System.Collections.Generic;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Initialize.Tags
{
    internal class Aspect : MethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Advices.OverrideMethod( builder.Target, nameof(OverrideMethod), new Dictionary<string, object?> { { "Friend", "Bernard" } } );
        }

        [Template]
        private dynamic? OverrideMethod()
        {
            Console.WriteLine( (string?)meta.Tags["Friend"] );

            return meta.Proceed();
        }
    }

    internal class TargetCode
    {
        // <target>
        [Aspect]
        private int Method( int a )
        {
            return a;
        }
    }
}