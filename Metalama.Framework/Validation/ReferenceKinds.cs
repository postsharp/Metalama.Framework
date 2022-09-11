// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.Validation
{
    /// <summary>
    /// Enumerates all kinds of references.
    /// </summary>
    /// <seealso href="@validation"/>
    [CompileTime]
    [Flags]
    public enum ReferenceKinds
    {
        None = 0,
        All = -1,
        BaseType = 1 << 0,
        MemberAccess = 1 << 1,
        TypeArgument = 1 << 2,
        TypeOf = 1 << 3,
        ParameterType = 1 << 4,
        TypeConstraint = 1 << 5,
        Other = 1 << 6,
        ObjectCreation = 1 << 7,
        FieldType = 1 << 8,
        LocalVariableType = 1 << 9,
        AttributeType = 1 << 10,
        ReturnType = 1 << 11,
        ArrayType = 1 << 12,
        NullableType = 1 << 13,
        PointerType = 1 << 14,
        RefType = 1 << 15,
        TupleType = 1 << 16,
        Invocation = 1 << 17,
        Assignment = 1 << 18
    }
}