#if TEST_OPTIONS
// @Skipped(constructed generics not supported)
# endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.BaseType_GenericSubstituted;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceClass(
            builder.Target,
            "TestNestedType",
            buildType: t =>
            {
                var typeParameter = t.AddTypeParameter( "U" );
                t.BaseType = builder.Target.WithTypeArguments( typeParameter );
            } );
    }
}

// <target>
[IntroductionAttribute]
public class TargetType<T> { }