using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.WithTemplateProvider;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.WithTemplateProvider( new TemplateProvider() ).IntroduceProperty( builder.Target, "MyProperty" );
    }
}

internal class TemplateProvider : ITemplateProvider
{
    [Template]
    public int MyProperty { get; set; }
}

[MyAspect]
public class C { }