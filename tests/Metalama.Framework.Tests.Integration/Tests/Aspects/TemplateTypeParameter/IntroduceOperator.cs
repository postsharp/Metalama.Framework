using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameters.IntroduceOperator;

#pragma warning disable CS0219

public class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        builder.Advice.IntroduceBinaryOperator(
            builder.Target,
            nameof(BinaryOperator),
            builder.Target,
            TypeFactory.GetType(typeof(int)),
            builder.Target,
            OperatorKind.Addition,
            args: new { T = builder.Target, x = 42 });

        builder.Advice.IntroduceUnaryOperator(
            builder.Target,
            nameof(UnaryOperator),
            builder.Target,
            builder.Target,
            OperatorKind.UnaryNegation,
            args: new { T = builder.Target, x = 42 });

        builder.Advice.IntroduceConversionOperator(
            builder.Target,
            nameof(ConversionOperator),
            builder.Target,
            TypeFactory.GetType(typeof(int)),
            true,
            args: new { T = builder.Target, x = 42 });
    }

    [Template]
    private T? BinaryOperator<[CompileTime] T>( [CompileTime] int x, T y, int p3 ) where T : class
    {
        return default;
    }

    [Template]
    private T? UnaryOperator<[CompileTime] T>([CompileTime] int x, T y) where T : class
    {
        return default;
    }

    [Template]
    private int ConversionOperator<[CompileTime] T>([CompileTime] int x, T y) where T : class
    {
        return default;
    }
}

// <target>
[Aspect]
public class Target { }