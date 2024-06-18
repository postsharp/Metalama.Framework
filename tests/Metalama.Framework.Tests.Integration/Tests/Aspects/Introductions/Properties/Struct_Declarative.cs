#if TEST_OPTIONS
// In C# 10, we need to generate slightly different code.
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
# endif

using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.Struct_Declarative
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public int IntroducedProperty { get; set; }

        [Introduce]
        public int IntroducedProperty_Initializer { get; set; } = 42;

        [Introduce]
        public static int IntroducedProperty_Static { get; set; }

        [Introduce]
        public static int IntroducedProperty_Static_Initializer { get; set; } = 42;
    }

    // <target>
    [Introduction]
    internal struct TargetStruct
    {
        private int _existingField;

        public TargetStruct( int x )
        {
            _existingField = x;
        }
    }
}