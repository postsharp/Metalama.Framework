using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0169, CS0414

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.UserDeclarativeAdvice_NonTemplate;

[AttributeUsage( AttributeTargets.Field )]
public class MyAdviceAttribute : DeclarativeAdviceAttribute
{
    public override void BuildAdvice( IMemberOrNamedType templateMember, string templateMemberId, IAspectBuilder<IDeclaration> builder )
    {
        builder.Advice.IntroduceField( (INamedType)builder.Target, "_field", typeof(string) );
    }
}

public class MyAspect : TypeAspect
{
    [MyAdvice]
    private RunTimeOnlyClass f = null!;
}

internal class RunTimeOnlyClass { }

// <target>
[MyAspect]
public class Target { }