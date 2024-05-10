using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Linking;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.TypeParameterExpression;

public class IntroductionAttribute : TypeAspect
{
    [Introduce]
    public void DeclarativeMethod<T>()
    {
        List<T> list = new();

        var expression = ExpressionFactory.Capture(list.Remove(default));

        meta.InsertStatement(expression);
    }
}

// <target>
[Introduction]
internal class TargetClass { }