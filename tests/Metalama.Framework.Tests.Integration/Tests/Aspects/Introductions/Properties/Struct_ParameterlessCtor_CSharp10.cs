#if TEST_OPTIONS
// @LanguageVersion(10)
# endif

using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.Struct_ParameterlessCtor_CSharp10;

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
    public TargetStruct() { }

    public int ExistingField = 42;

    public int ExistingProperty { get; set; } = 42;
}