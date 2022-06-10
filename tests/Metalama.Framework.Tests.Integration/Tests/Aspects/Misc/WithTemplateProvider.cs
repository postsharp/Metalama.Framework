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
    public string MyProperty => meta.Target.Type.Name;
}

// <target>
[MyAspect]
public class C { }