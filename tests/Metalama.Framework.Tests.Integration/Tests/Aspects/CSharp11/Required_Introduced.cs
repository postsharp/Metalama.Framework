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