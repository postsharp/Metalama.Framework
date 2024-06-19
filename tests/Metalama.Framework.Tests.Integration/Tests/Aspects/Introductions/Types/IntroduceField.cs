using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.IntroduceField;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var result = builder.IntroduceClass( "TestNestedType" );

        builder.With( result.Declaration ).IntroduceField( nameof(Field) );
    }

    [Template]
    public int Field;
}

// <target>
[IntroductionAttribute]
public class TargetType { }