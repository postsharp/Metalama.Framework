#if TEST_OPTIONS
// @ClearIgnoredDiagnostics to verify nullability warnings
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.DynamicParameterIntroduction;

internal class Aspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var objectType = TypeFactory.GetType(typeof(object));

        builder.Advice.IntroduceMethod(
            builder.Target, nameof(Template),
            buildMethod: methodBuilder =>
            {
                methodBuilder.Name = "Nullable";
                methodBuilder.Parameters.Single().Type = objectType.ToNullableType();
            });

        builder.Advice.IntroduceMethod(
            builder.Target, nameof(Template),
            buildMethod: methodBuilder =>
            {
                methodBuilder.Name = "NonNullable";
                methodBuilder.Parameters.Single().Type = objectType.ToNonNullableType();
            });

        builder.Advice.IntroduceMethod(
            builder.Target, nameof(Template),
            buildMethod: methodBuilder =>
            {
                methodBuilder.Name = "Default";
                methodBuilder.Parameters.Single().Type = objectType;
            });
    }

    [Template]
    string Template(dynamic? arg) => arg?.ToString() + arg!.ToString();
}

// <target>
[Aspect]
class TargetCode
{
}