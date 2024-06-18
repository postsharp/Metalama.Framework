using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.AddAspect.AddChildAspectError;

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(Aspect1), typeof(Aspect2) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.AddAspect.AddChildAspectError
{
    internal class Aspect1 : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Outbound.SelectMany( t => t.Methods ).AddAspect( _ => new Aspect2( "Hello, world." ) );
        }
    }

    internal class Aspect2 : OverrideMethodAspect
    {
        private string _value;

        public Aspect2( string value )
        {
            _value = value;
        }

        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( _value );

            return meta.Proceed();
        }
    }

    [Aspect1]
    internal class TargetCode
    {
        // <target>
        private int Method( int a )
        {
            return a;
        }
    }
}