using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.CopyAttributeToIntroducedProperty
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        [Foo]
        public int IntroducedProperty_Auto
        {
            [return: Foo]
            get;
            [return: Foo]
            [param: Foo]
            set;
        }

        [Introduce]
        [Foo]
        public int IntroducedProperty_Auto_Initializer
        {
            [return: Foo]
            get;
            [return: Foo]
            [param: Foo]
            set;
        } = 42;

        [Introduce]
        [Foo]
        public int IntroducedProperty_Accessors
        {
            [return: Foo]
            get
            {
                Console.WriteLine( "Get" );

                return 42;
            }

            [return: Foo]
            [param: Foo]
            set
            {
                Console.WriteLine( value );
            }
        }
    }

    public class FooAttribute : Attribute { }

    // <target>
    [Introduction]
    internal class TargetClass { }
}