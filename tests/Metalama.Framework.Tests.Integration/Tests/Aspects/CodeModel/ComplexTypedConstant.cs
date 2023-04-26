using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Aspects.CodeModel.ComplexTypedConstant;

class Aspect : TypeAspect
{
    [Template]
    object[] P { get; } = null!;

    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var typedConstant = TypedConstant.Create(new object[] { new[] { ConsoleColor.Red }, new object[] { ConsoleColor.Red } });
        builder.Advice.IntroduceField(builder.Target, "f", typeof(object[]), buildField: field => field.InitializerExpression = typedConstant);
        builder.Advice.IntroduceProperty(builder.Target, nameof(P), buildProperty: property => property.InitializerExpression = typedConstant);

        var attributeConstructor = ((INamedType)TypeFactory.GetType(typeof(MyAttribute))).Constructors.Single();
        builder.Advice.IntroduceAttribute(builder.Target, AttributeConstruction.Create(attributeConstructor, new[] { typedConstant }));
    }
}

class MyAttribute : Attribute
{
    public MyAttribute(object[] array) { }
}

// <target>
[Aspect]
class TargetCode { }