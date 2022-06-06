using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Properties.AutomaticProperty;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        // Default
        builder.Advice.IntroduceAutomaticProperty( builder.Target, "P1", typeof(int) );

        // Change property visibility.
        var property2 = builder.Advice.IntroduceAutomaticProperty( builder.Target, "P2", typeof(int) );
        property2.Accessibility = Accessibility.Protected;

        // Change accessor visibility.
        var property3 = builder.Advice.IntroduceAutomaticProperty( builder.Target, "P3", typeof(int) );
        property3.Accessibility = Accessibility.Public;
        property3.SetMethod.Accessibility = Accessibility.Protected;
    }
}

[MyAspect]
public class C { }