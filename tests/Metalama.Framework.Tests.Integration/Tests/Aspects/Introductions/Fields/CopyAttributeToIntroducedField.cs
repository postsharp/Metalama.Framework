using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.IntegrationTests.Aspects.Introductions.Field.CopyAttributeToIntroducedField;

[assembly: AspectOrder( typeof(IntroductionAttribute), typeof(OverrideAttribute) )]

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Field.CopyAttributeToIntroducedField
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public int IntroducedField;

        [Introduce]
        public int IntroducedField_Initializer = 42;

        [Introduce]
        public static int IntroducedField_Static;

        [Introduce]
        public static int IntroducedField_Static_Initializer = 42;
    }

    public class OverrideAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get
            {
                Console.WriteLine("Overriden.");
                return meta.Proceed();
            }
            set
            {
                Console.WriteLine("Overriden.");
                meta.Proceed();
            }
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}