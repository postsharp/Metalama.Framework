using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.SystemTypeNullability;

internal class Aspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        var defaultField = builder.Advice.IntroduceField(builder.Target, "Default", typeof(object)).Declaration;

        builder.Advice.IntroduceMethod(builder.Target, nameof(CallToString), buildMethod: methodBuilder => methodBuilder.Name = "DefaultToString", args: new { target = defaultField });

        var nullableField = builder.Advice.IntroduceField(builder.Target, "Nullable", typeof(object).ToNullableType()).Declaration;

        builder.Advice.IntroduceMethod(builder.Target, nameof(CallToString), buildMethod: methodBuilder => methodBuilder.Name = "NullableToString", args: new { target = nullableField });
    }

    [Template]
    public string CallToString(IField target) => target.Value!.ToString();
}

// <target>
[Aspect]
class TargetCode { }