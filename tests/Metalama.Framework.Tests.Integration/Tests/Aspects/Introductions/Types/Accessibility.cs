using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.Accessibility;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceClass( builder.Target, "PrivateType", TypeKind.Class, buildType: t => { t.Accessibility = Code.Accessibility.Private; } );
        builder.Advice.IntroduceClass( builder.Target, "ProtectedType", TypeKind.Class, buildType: t => { t.Accessibility = Code.Accessibility.Protected; } );

        builder.Advice.IntroduceClass(
            builder.Target,
            "PrivateProtectedType",
            TypeKind.Class,
            buildType: t => { t.Accessibility = Code.Accessibility.PrivateProtected; } );

        builder.Advice.IntroduceClass(
            builder.Target,
            "ProtectedInternalType",
            TypeKind.Class,
            buildType: t => { t.Accessibility = Code.Accessibility.ProtectedInternal; } );

        builder.Advice.IntroduceClass( builder.Target, "InternalType", TypeKind.Class, buildType: t => { t.Accessibility = Code.Accessibility.Internal; } );
        builder.Advice.IntroduceClass( builder.Target, "PublicType", TypeKind.Class, buildType: t => { t.Accessibility = Code.Accessibility.Public; } );
        builder.Advice.IntroduceClass( builder.Target, "UndefinedType", TypeKind.Class, buildType: t => { t.Accessibility = Code.Accessibility.Undefined; } );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType { }