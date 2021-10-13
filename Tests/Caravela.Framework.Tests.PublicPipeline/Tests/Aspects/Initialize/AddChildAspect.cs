using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Tests.Integration.Aspects.Initialize.AddChildAspect;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1) )]

namespace Caravela.Framework.Tests.Integration.Aspects.Initialize.AddChildAspect
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
            Console.WriteLine( meta.AspectInstance.Predecessors.Single().Instance.ToString() );

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