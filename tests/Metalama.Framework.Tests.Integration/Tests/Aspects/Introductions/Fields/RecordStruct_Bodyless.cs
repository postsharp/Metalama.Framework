#if TEST_OPTIONS
// In C# 10, we need to generate slightly different code.
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Field.RecordStruct_Bodyless
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

    // <target>
    [Introduction]
    internal record struct TargetRecordStruct;
}