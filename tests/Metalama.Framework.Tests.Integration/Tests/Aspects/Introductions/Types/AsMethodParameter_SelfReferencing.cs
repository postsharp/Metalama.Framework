﻿using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Types.AsMethodParameter_SelfReferencing;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var result = builder.IntroduceClass( "IntroducedNestedType" );

        builder.With( result.Declaration )
            .IntroduceMethod(
                nameof(MethodTemplate),
                buildMethod: b =>
                {
                    b.Name = "Method";
                    b.AddParameter( "p", result.Declaration );
                } );
    }

    [Template]
    public void MethodTemplate() { }
}

// <target>
[IntroductionAttribute]
public class TargetType { }