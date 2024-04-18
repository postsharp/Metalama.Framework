// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using System.Collections.Generic;

namespace Metalama.Framework.Validation;

/// <summary>
/// Extension methods for <see cref="ReferenceKinds"/>.
/// </summary>
[PublicAPI]
public static class ReferenceKindsExtension
{
    /// <summary>
    /// Same as <c>ToString</c> excepts that it does not use obsolete names.
    /// </summary>
    public static string ToDisplayString( this ReferenceKinds kinds )
    {
        // We have some non-standard handling because of obsolete members.
        // The behavior of Enum.ToString is random where there are synonyms.

        switch ( kinds )
        {
            case ReferenceKinds.All:
                return "All";

            case ReferenceKinds.None:
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
        ConsiderKind( ReferenceKinds.ArrayElementType );
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

    /// <summary>
    /// Determines if a <see cref="ReferenceKinds"/> contains all required flags.
    /// </summary>
    public static bool IsDefined( this ReferenceKinds kinds, ReferenceKinds requiredKinds ) => (kinds & requiredKinds) == requiredKinds;
}