#if TEST_OPTIONS
// @ClearIgnoredDiagnostics to verify nullability warnings
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.DynamicParameterQuestionMarkTypes;

internal class Aspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.Override(builder.Target, nameof(Template));
    }

    [Template]
    void Template(dynamic? arg) => arg?.ToString();
}

// <target>
class TargetCode
{
    class Nullable
    {
        [Aspect]
        void ValueType(int arg) { }

        [Aspect]
        void NullableValueType(int? arg) { }

        [Aspect]
        void ReferenceType(string arg) { }

        [Aspect]
        void NullableReferenceType(string? arg) { }

        [Aspect]
        void Generic<T>(T arg) { }

        [Aspect]
        void NullableGeneric<T>(T? arg) { }

        [Aspect]
        void NotNullGeneric<T>(T arg) where T : notnull { }

        [Aspect]
        void NullableNotNullGeneric<T>(T? arg) where T : notnull { }

        [Aspect]
        void ValueTypeGeneric<T>(T arg) where T : struct { }

        [Aspect]
        void NullableValueTypeGeneric<T>(T? arg) where T : struct { }

        [Aspect]
        void ReferenceTypeGeneric<T>(T arg) where T : class { }

        [Aspect]
        void NullableReferenceTypeGeneric<T>(T? arg) where T : class { }

        [Aspect]
        void ReferenceTypeNullableGeneric<T>(T arg) where T : class? { }

        [Aspect]
        void NullableReferenceTypeNullableGeneric<T>(T? arg) where T : class? { }

        [Aspect]
        void SpecificReferenceTypeGeneric<T>(T arg) where T : IComparable { }

        [Aspect]
        void SpecificNullableReferenceTypeGeneric<T>(T? arg) where T : IComparable { }

        [Aspect]
        void SpecificReferenceTypeNullableGeneric<T>(T arg) where T : IComparable? { }

        [Aspect]
        void SpecificNullableReferenceTypeNullableGeneric<T>(T? arg) where T : IComparable? { }

    }

#nullable disable

    class NonNullable
    {
        [Aspect]
        void ValueType(int arg) { }

        [Aspect]
        void NullableValueType(int? arg) { }

        [Aspect]
        void ReferenceType(string arg) { }

        [Aspect]
        void Generic<T>(T arg) { }

        [Aspect]
        void ValueTypeGeneric<T>(T arg) where T : struct { }

        [Aspect]
        void NullableValueTypeGeneric<T>(T? arg) where T : struct { }

        [Aspect]
        void ReferenceTypeGeneric<T>(T arg) where T : class { }

        [Aspect]
        void SpecificReferenceTypeGeneric<T>(T arg) where T : IComparable { }
    }
}