#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

#if ROSLYN_4_4_0_OR_GREATER

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp11.RefMethod_Introduction_NotSupported;

public class TheAspect : TypeAspect
{
    [Introduce]
    private int _x;

    [Introduce]
    public ref int GetRef() => ref _x;
}

[TheAspect]
internal class C
{
    
}

#endif