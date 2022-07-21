using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.CopyAttributeToIntroducedProperty;

[assembly: AspectOrder( typeof(IntroduceAttribute), typeof(OverrideAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.CopyAttributeToIntroducedProperty
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        [Override]
        public int IntroducedProperty_Auto { get; set; }

        [Introduce]
        [Override]
        public int IntroducedProperty_Auto_Initializer { get; set; } = 42;

        [Introduce]
        [Override]
        public int IntroducedProperty_Auto_GetOnly { get; }

        [Introduce]
        [Override]
        public int IntroducedProperty_Auto_GetOnly_Initializer { get; } = 42;

        [Introduce]
        [Override]
        public static int IntroducedProperty_Auto_Static { get; set; }

        [Introduce]
        [Override]
        public int IntroducedProperty_Accessors
        {
            get
            {
                Console.WriteLine( "Get" );

                return 42;
            }

            set
            {
                Console.WriteLine( value );
            }
        }
    }

    public class OverrideAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get
            {
                Console.WriteLine("This is the overridden getter.");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine($"This is the overridden setter.");
                meta.Proceed();
            }
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}