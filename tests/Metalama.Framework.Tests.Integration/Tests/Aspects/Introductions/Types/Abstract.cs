using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.Abstract;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var introducedType = builder.IntroduceClass( "SealedType", TypeKind.Class, buildType: t => { t.IsAbstract = true; } );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType { }