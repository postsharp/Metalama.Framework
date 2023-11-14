#if TEST_OPTIONS 
// @RequiredConstant(NET7_0_OR_GREATER)
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

#if NET7_0_OR_GREATER && ROSLYN_4_4_0_OR_GREATER

using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp11.Required_Introduced;

public class TheAspect : TypeAspect
{
    [Introduce( IsRequired = true )]
    public int Field;

    [Introduce( IsRequired = true )]
    public int Property { get; set; }
}

// <target>
[TheAspect]
public class C { }

#endif