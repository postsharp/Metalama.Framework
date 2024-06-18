using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.BaseType_Generic;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.IntroduceClass(
            "TestNestedType",
            buildType: t => { t.BaseType = builder.Target.WithTypeArguments( TypeFactory.GetType( SpecialType.Object ) ); } );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType<T> { }