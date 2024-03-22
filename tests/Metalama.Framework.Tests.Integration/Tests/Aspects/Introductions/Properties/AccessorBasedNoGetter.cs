using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Properties.AccessorBasedNoGetter
{
    internal class MyAspect : TypeAspect
    {
        [Template]
        public void Setter( int value )
        {
            Console.WriteLine( "Introduced" );
        }

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.IntroduceProperty( builder.Target, "TheProperty", null, nameof(Setter) );
        }
    }

    // <target>
    [MyAspect]
    internal class C { }
}