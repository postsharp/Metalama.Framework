#if TEST_OPTIONS
// @LanguageVersion(8.0)
#endif
using Metalama.Framework.Aspects;

// This reproduces #30669.

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.LanguageVersion
{
    internal class MyAspect : TypeAspect { }

    // <target>
    [MyAspect]
    internal class C { }
}