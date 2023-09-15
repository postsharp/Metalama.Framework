using Metalama.Framework.Tests.Integration.Tests.Options;
#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

[assembly: MyOptions( "Attribute.Assembly" )]

namespace Metalama.Framework.Tests.Integration.Tests.Options.Attributes;

[MyOptions( "Attribute.C" )]
[OptionsAspect]
public class C
{
    [OptionsAspect]
    [MyOptions( "Attribute.M" )]
    public void M( int p ) { }
}

[OptionsAspect]
public class D { }