using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Properties.AutomaticProperty;

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        // Default
        builder.Advise.IntroduceAutomaticProperty( builder.Target, "P1", typeof(int) );

        // Change property visibility.
        builder.Advise.IntroduceAutomaticProperty( builder.Target, "P2", typeof(int), buildProperty: p => { p.Accessibility = Accessibility.Protected; } );

        // Change accessor visibility.
        builder.Advise.IntroduceAutomaticProperty(
            builder.Target,
            "P3",
            typeof(int),
            buildProperty: p =>
            {
                p.Accessibility = Accessibility.Public;
                p.SetMethod!.Accessibility = Accessibility.Protected;
            } );
    }
}

// <target>
[MyAspect]
public class C { }