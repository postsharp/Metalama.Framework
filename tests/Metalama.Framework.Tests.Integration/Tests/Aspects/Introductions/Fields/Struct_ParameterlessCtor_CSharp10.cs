#if TEST_OPTIONS
// @LanguageVersion(10)
# endif

using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Field.Struct_ParameterlessCtor_CSharp10;

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
internal struct TargetStruct
{
    public TargetStruct() { }

    public int ExistingField = 42;

    public int ExistingProperty { get; set; } = 42;
}