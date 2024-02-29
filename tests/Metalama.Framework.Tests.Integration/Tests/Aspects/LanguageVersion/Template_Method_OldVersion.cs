#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
// @LanguageVersion(10)
// @AcceptInvalidInput
#endif

#if ROSLYN_4_4_0_OR_GREATER

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LanguageVersion.Template_Method_OldVersion;

// <target>
[TheAspect]
class Target
{
}

#endif