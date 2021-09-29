using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Tests.Integration.Aspects.Initialize.AddChildAspectError;

[assembly: AspectOrder( typeof(Aspect1), typeof(Aspect2) )]

namespace Caravela.Framework.Tests.Integration.Aspects.Initialize.AddChildAspectError
{
    internal class Aspect1 : Attribute, IAspect<INamedType>
    {
        public void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.WithMembers( t => t.Methods ).AddAspect( _ => new Aspect2( "Hello, world." ) );
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