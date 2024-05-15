#if TESTOPTIONS
// @Skipped(constructed generics not supported)
# endif

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.BaseType_SelfReferencing;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceType(
            builder.Target, 
            "TestNestedType", 
            TypeKind.Class, 
            buildType: t => { t.BaseType = builder.Target.WithTypeArguments(t); });
    }
}


// <target>
[IntroductionAttribute]
public class TargetType<T>
{
}