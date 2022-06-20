using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.UserDeclarativeAdvice;

[AttributeUsage( AttributeTargets.Field )]
public class MyAdviceAttribute : DeclarativeAdviceAttribute, ITemplateAttribute
{
    string? ITemplateAttribute.Name => null;
    bool? ITemplateAttribute.IsVirtual => null;
    bool? ITemplateAttribute.IsSealed => null;

    Accessibility? ITemplateAttribute.Accessibility { get; }

    public override void BuildAdvice( IMemberOrNamedType templateMember, string templateMemberId, IAspectBuilder<IDeclaration> builder )
    {
        builder.Advice.IntroduceField( (INamedType) builder.Target, templateMemberId );
    }
}

public class MyAspect : TypeAspect
{
    [MyAdvice]
    private int f;
}

// <target>
[MyAspect]
public class Target { }