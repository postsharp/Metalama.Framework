using Metalama.Framework.Tests.Integration.Tests.Options;

namespace Metalama.Framework.Tests.Integration.Tests.Options.BaseType;
[MyOptions("Attribute.C")]
[OptionsAspect]
[global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("Attribute.C")]
public class C
{
}

[OptionsAspect]
[global::Metalama.Framework.Tests.Integration.Tests.Options.ActualOptionsAttribute("Attribute.C")]
public class D : C
{
}
