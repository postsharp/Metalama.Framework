#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
// @LanguageVersion(10)
#endif

#if ROSLYN_4_4_0_OR_GREATER

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LanguageVersion.Template_OldVersion;

class Target
{
    // <target>
    [TheAspect]
    void M()
    {

    }
}

#endif