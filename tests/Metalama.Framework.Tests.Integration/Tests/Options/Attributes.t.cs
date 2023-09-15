using Metalama.Framework.Tests.Integration.Tests.Options;

[assembly: MyOptions("Attribute.Assembly")]
namespace Metalama.Framework.Tests.Integration.Tests.Options.Attributes;
[MyOptions("Attribute.C")]
[OptionsAspect]
[global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("Attribute.C")]
public class C
{
    [OptionsAspect]
    [MyOptions("Attribute.M")]
    [global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("Attribute.M")]
    public void M(int p)
    {
    }
}

[OptionsAspect]
[global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("Attribute.Assembly")]
public class D
{
}
