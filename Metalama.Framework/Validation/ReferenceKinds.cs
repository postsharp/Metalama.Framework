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
    public enum ReferenceKinds : long
    {
        None = 0,

        /// <summary>
        /// All reference kinds.
        /// </summary>
        All = -1,

        /// <summary>
        /// A field or property access that does not fall into another category.
        /// </summary>
        Default = 1 << 0,

        /// <summary>
        /// Base type or interface.
        /// </summary>
        BaseType = 1 << 1,

        /// <summary>
        /// Type argument (e.g. generic argument). 
        /// </summary>
        TypeArgument = 1 << 2,

        /// <summary>
        /// <c>typeof</c>.
        /// </summary>
        TypeOf = 1 << 3,

        /// <summary>
        /// Parameter reference.
        /// </summary>
        ParameterType = 1 << 4,

        /// <summary>
        /// Type constraint of a generic parameter.
        /// </summary>
        TypeConstraint = 1 << 5,

        /// <summary>
        /// Object construction, i.e. constructor invocation.
        /// </summary>
        ObjectCreation = 1 << 7,

        /// <summary>
        /// Type of a field, property, or event.
        /// </summary>
        MemberType = 1 << 8,

        /// <summary>
        /// Type of a local variable.
        /// </summary>
        LocalVariableType = 1 << 9,

        /// <summary>
        /// Type of a custom attribute.
        /// </summary>
        AttributeType = 1 << 10,

        /// <summary>
        /// Return type of a method.
        /// </summary>
        ReturnType = 1 << 11,

        /// <summary>
        /// Element type of an array.
        /// </summary>
        ArrayType = 1 << 12,

        /// <summary>
        /// Nullable type.
        /// </summary>
        NullableType = 1 << 13,

        /// <summary>
        /// Type of element pointed at by an unmanaged pointer.
        /// </summary>
        PointerType = 1 << 14,

        /// <summary>
        /// Type of a <c>ref</c>.
        /// </summary>
        RefType = 1 << 15,

        /// <summary>
        /// Type of a tuple element.
        /// </summary>
        TupleType = 1 << 16,

        /// <summary>
        /// Invocation of a method or delegate.
        /// </summary>
        Invocation = 1 << 17,

        /// <summary>
        /// Left part of an assignment.
        /// </summary>
        Assignment = 1 << 18,

        /// <summary>
        /// The member is being overridden using an <c>override</c>.
        /// </summary>
        OverrideMember = 1 << 19,

        /// <summary>
        /// Implicit or explicit implementation of an interface member.
        /// </summary>
        InterfaceMemberImplementation = 1 << 20,

        /// <summary>
        /// Inside a <c>using</c> directive.
        /// </summary>
        Using = 1 << 21,

        /// <summary>
        /// <c>nameof</c>.
        /// </summary>
        NameOf = 1 << 22
    }
}