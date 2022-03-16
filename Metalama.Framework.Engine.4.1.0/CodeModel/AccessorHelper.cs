// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Engine.CodeModel
{
    internal static class AccessorHelper
    {
        public static string ToDisplayString(
            this IMember declaringMember,
            MethodKind methodKind,
            CodeDisplayFormat? format = null,
            CodeDisplayContext? context = null )
            => declaringMember.ToDisplayString( format, context ) + "." + methodKind switch
            {
                MethodKind.EventAdd => "add",
                MethodKind.EventRemove => "remove",
                MethodKind.PropertyGet => "get",
                MethodKind.PropertySet => "set",
                MethodKind.EventRaise => "raise",
                _ => throw new ArgumentOutOfRangeException( nameof(methodKind) )
            };
    }
}