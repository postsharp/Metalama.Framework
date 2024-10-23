// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Engine.CodeModel.Helpers
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