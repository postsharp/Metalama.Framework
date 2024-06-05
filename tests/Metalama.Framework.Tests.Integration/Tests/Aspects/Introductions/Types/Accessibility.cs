using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.Accessibility;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceClass( builder.Target, "PrivateType", buildType: t => { t.Accessibility = Code.Accessibility.Private; } );
        builder.Advice.IntroduceClass( builder.Target, "ProtectedType", buildType: t => { t.Accessibility = Code.Accessibility.Protected; } );

        builder.Advice.IntroduceClass(
            builder.Target,
            "PrivateProtectedType",
            buildType: t => { t.Accessibility = Code.Accessibility.PrivateProtected; } );

        builder.Advice.IntroduceClass(
            builder.Target,
            "ProtectedInternalType",
            buildType: t => { t.Accessibility = Code.Accessibility.ProtectedInternal; } );

        builder.Advice.IntroduceClass( builder.Target, "InternalType", buildType: t => { t.Accessibility = Code.Accessibility.Internal; } );
        builder.Advice.IntroduceClass( builder.Target, "PublicType", buildType: t => { t.Accessibility = Code.Accessibility.Public; } );
        builder.Advice.IntroduceClass( builder.Target, "UndefinedType", buildType: t => { t.Accessibility = Code.Accessibility.Undefined; } );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType { }