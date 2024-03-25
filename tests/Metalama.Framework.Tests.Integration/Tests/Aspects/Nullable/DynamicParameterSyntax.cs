#if TEST_OPTIONS
// @ClearIgnoredDiagnostics to verify nullability warnings
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.DynamicParameterQuestionMarkSyntax;

internal class Aspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.Override(builder.Target, nameof(Template));
    }

    [Template]
    void Template(dynamic? arg)
    {
        var s = arg?.Nullable?.ToString();
        s = arg?.NonNullable?.ToString();

        s = arg!.Nullable!.ToString();
        s = arg!.NonNullable!.ToString();

        var i = arg?[0]?[1];
        i = arg![0]![1];
    }
}

class Foo
{
    public object? Nullable = null;
    public object NonNullable = null!;

    public int[] this[int i] => null!;
}

// <target>
class TargetCode
{
    class Nullable
    {
        [Aspect]
        void ReferenceType(Foo arg) { }

        [Aspect]
        void NullableReferenceType(Foo? arg) { }
    }

#nullable disable

    class NonNullable
    {
        [Aspect]
        void ReferenceType(Foo arg) { }
    }
}