#if TEST_OPTIONS
// @ClearIgnoredDiagnostics to verify nullability warnings
#endif

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.DynamicMetaParameterQuestionMark;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod() => meta.Target.Parameters[0].Value?.ToString();
}

// <target>
class TargetCode
{
    class Nullable
    {
        [Aspect]
        void ValueType(int i) { }

        [Aspect]
        void NullableValueType(int? i) { }

        [Aspect]
        void ReferenceType(string s) { }

        [Aspect]
        void NullableReferenceType(string? s) { }

        [Aspect]
        void Generic<T>(T t) { }

        [Aspect]
        void NullableGeneric<T>(T? t) { }

        [Aspect]
        void NotNullGeneric<T>(T t) where T : notnull { }

        [Aspect]
        void NullableNotNullGeneric<T>(T? t) where T : notnull { }

        [Aspect]
        void ValueTypeGeneric<T>(T t) where T : struct { }

        [Aspect]
        void NullableValueTypeGeneric<T>(T? t) where T : struct { }

        [Aspect]
        void ReferenceTypeGeneric<T>(T t) where T : class { }

        [Aspect]
        void NullableReferenceTypeGeneric<T>(T? t) where T : class { }


        [Aspect]
        void ReferenceTypeNullableGeneric<T>(T t) where T : class? { }

        [Aspect]
        void NullableReferenceTypeNullableGeneric<T>(T? t) where T : class? { }

        [Aspect]
        void SpecificReferenceTypeGeneric<T>(T t) where T : IComparable { }

        [Aspect]
        void SpecificNullableReferenceTypeGeneric<T>(T? t) where T : IComparable { }

        [Aspect]
        void SpecificReferenceTypeNullableGeneric<T>(T t) where T : IComparable? { }

        [Aspect]
        void SpecificNullableReferenceTypeNullableGeneric<T>(T? t) where T : IComparable? { }
    }

#nullable disable

    class NonNullable
    {
        [Aspect]
        void ValueType(int i) { }

        [Aspect]
        void NullableValueType(int? i) { }

        [Aspect]
        void ReferenceType(string s) { }

        [Aspect]
        void Generic<T>(T t) { }

        [Aspect]
        void ValueTypeGeneric<T>(T t) where T : struct { }

        [Aspect]
        void NullableValueTypeGeneric<T>(T? t) where T : struct { }

        [Aspect]
        void ReferenceTypeGeneric<T>(T t) where T : class { }

        [Aspect]
        void SpecificReferenceTypeGeneric<T>(T t) where T : IComparable { }

    }
}