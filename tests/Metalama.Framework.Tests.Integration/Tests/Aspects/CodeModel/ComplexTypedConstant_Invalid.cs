#if TESTOPTIONS
// @AcceptInvalidInput
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Aspects.CodeModel.ComplexTypedConstant_Invalid;

public class Aspect : TypeAspect
{
    [Template]
    int[] P { get; } = null!;

    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var typedConstant = TypedConstant.Create(new[] { 42 });
        builder.Advice.IntroduceParameter(builder.Target.Constructors.Single(), "p", typeof(int[]), typedConstant);
    }
}

class MyAttribute : Attribute
{
    public MyAttribute(int[] array) { }
}

// <target>
[Aspect]
class TargetCode { }