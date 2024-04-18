// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Validation
{
    public static class ReferenceKindsExtension
    {
        public static string ToDisplayString( this ReferenceKinds kinds )
        {
            // We have some non-standard handling because of obsolete members.

            if ( kinds == ReferenceKinds.All )
            {
                return "All";
            }
            else if ( kinds == ReferenceKinds.None )
            {
                return "None";
            }

            List<string> values = new();
            var consideredKinds = ReferenceKinds.None;

            void ConsiderKind( ReferenceKinds kind )
            {
                if ( kind.IsDefined( kind ) )
                {
                    values.Add( kind.ToString() );
                    consideredKinds |= kind;
                }
            }

            ConsiderKind( ReferenceKinds.Default );
            ConsiderKind( ReferenceKinds.BaseType );
            ConsiderKind( ReferenceKinds.TypeArgument );
            ConsiderKind( ReferenceKinds.TypeOf );
            ConsiderKind( ReferenceKinds.ParameterType );
            ConsiderKind( ReferenceKinds.TypeConstraint );
            ConsiderKind( ReferenceKinds.ObjectCreation );
            ConsiderKind( ReferenceKinds.MemberType );
            ConsiderKind( ReferenceKinds.LocalVariableType );
            ConsiderKind( ReferenceKinds.ReturnType );
            ConsiderKind( ReferenceKinds.ArrayElementType );
            ConsiderKind( ReferenceKinds.PointerType );
            ConsiderKind( ReferenceKinds.TupleElementType );
            ConsiderKind( ReferenceKinds.Invocation );
            ConsiderKind( ReferenceKinds.Assignment );
            ConsiderKind( ReferenceKinds.OverrideMember );
            ConsiderKind( ReferenceKinds.Using );
            ConsiderKind( ReferenceKinds.NameOf );
            ConsiderKind( ReferenceKinds.BaseConstructor );

            if ( consideredKinds != kinds )
            {
                // If we forgot something, fallback to ToString.
                return kinds.ToString();
            }

            return string.Join( " | ", values );
        }

        public static bool IsDefined( this ReferenceKinds kinds, ReferenceKinds kind ) => (kinds & kind) == kind;
    }

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
        ArrayElementType = 1 << 12,

        [Obsolete( "Renamed to ArrayElementType." )]
        ArrayType = ArrayElementType,

        /// <summary>
        /// Nullable type.
        /// </summary>
        [Obsolete( "No longer detected." )]
        NullableType = 1 << 13,

        /// <summary>
        /// Type of element pointed at by an unmanaged pointer.
        /// </summary>
        PointerType = 1 << 14,

        /// <summary>
        /// Type of a <c>ref</c>.
        /// </summary>
        [Obsolete( "No longer detected." )]
        RefType = 1 << 15,

        [Obsolete( "Renamed to TypleElementType." )]
        TupleType = TupleElementType,

        /// <summary>
        /// Type of a tuple element.
        /// </summary>
        TupleElementType = 1 << 16,

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
        NameOf = 1 << 22,

        /// <summary>
        /// Base constructor (either <c>this</c> or <c>base</c>).
        /// </summary>
        BaseConstructor = 1 << 23
    }
}