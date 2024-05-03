#if TESTOPTIONS
// @Skipped(constructed generics not supported)
# endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.BaseType_GenericSubstituted;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceType(
            builder.Target.ForCompilation(builder.Advice.MutableCompilation), 
            "TestNestedType", 
            TypeKind.Class, 
            buildType: t => 
            {
                var typeParameter = t.AddTypeParameter("U");
                t.BaseType = builder.Target.WithTypeArguments(typeParameter); 
            });
    }
}

// <target>
[IntroductionAttribute]
public class TargetType<T>
{
}