using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialize.Tags
{
    internal class Aspect : MethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Advices.Override( builder.Target, nameof(OverrideMethod), new Framework.Aspects.TagDictionary { { "Friend", "Bernard" } } );
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