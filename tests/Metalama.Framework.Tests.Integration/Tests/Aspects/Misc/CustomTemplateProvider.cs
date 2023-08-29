using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.CustomTemplateProvider;

internal class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.WithTemplateProvider( TemplateProvider.FromInstance( new MyTemplateProvider() ) )
            .IntroduceMethod( builder.Target, nameof(MyTemplateProvider.NewMethod) );
    }
}

[TemplateProvider]
internal class MyTemplateProvider
{
    [Template]
    public void NewMethod()
    {
        Console.WriteLine( "Hello, world." );
    }
}

// <target>
[MyAspect]
internal class Target { }