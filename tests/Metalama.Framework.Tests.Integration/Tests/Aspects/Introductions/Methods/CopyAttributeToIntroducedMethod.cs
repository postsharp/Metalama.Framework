using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.CopyAttributeToIntroducedMethod
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        [Foo]
        [return: Foo]
        public void IntroducedMethod_Void<[Foo] T>( [Foo] int i )
        {
            Console.WriteLine( "This is introduced method." );
        }
    }

    public class FooAttribute : Attribute { }

    // <target>
    [Introduction]
    internal class TargetClass { }
}