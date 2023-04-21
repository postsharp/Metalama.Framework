using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Fields.FieldModifiers;

public class Aspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceField(builder.Target, "unmodifiedField", typeof(int), buildField: field => field.Writeability = Writeability.All);
        builder.Advice.IntroduceField(builder.Target, "readonlyField", typeof(int), buildField: field => field.Writeability = Writeability.ConstructorOnly);
        builder.Advice.IntroduceField(builder.Target, "constField", typeof(int), buildField: field =>
        {
            field.Writeability = Writeability.None;
            field.InitializerExpression = ExpressionFactory.Capture(42);
        });
    }
}

// <target>
[Aspect]
class TargetClass { }