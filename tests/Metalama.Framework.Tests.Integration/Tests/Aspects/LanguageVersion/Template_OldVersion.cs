#if TEST_OPTIONS
// @LanguageVersion(10)
#endif

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LanguageVersion.Template_OldVersion;

class Target
{
    // <target>
    [TheAspect]
    void M()
    {

    }
}