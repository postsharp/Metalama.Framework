using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Options;
using Metalama.Framework.Code.DeclarationBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Options;

#if TEST_OPTIONS
// @Include(_Common.cs)
#endif

public class TheAspect : TypeAspect, IHierarchicalOptionsProvider<MyOptions>
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var options = builder.AspectInstance.GetOptions<MyOptions>();

        builder.Advice.IntroduceAttribute(
            builder.Target,
            AttributeConstruction.Create( typeof(ActualOptionsAttribute), new[] { options.OverrideHistory } ) );
    }

    public MyOptions GetOptions()
    {
        return new MyOptions { Value = "FromTheAspect" };
    }
}

// <target>
[MyOptions( "FromAttribute" )]
[TheAspect]
public class C { }