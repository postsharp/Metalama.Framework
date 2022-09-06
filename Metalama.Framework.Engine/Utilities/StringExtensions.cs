// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Text;

namespace Metalama.Framework.Engine.Utilities
{
    internal static class StringExtensions
    {
        public static string TrimSuffix( this string s, string suffix )
            => s.EndsWith( suffix, StringComparison.Ordinal ) ? s.Substring( 0, s.Length - suffix.Length ) : s;

        public static void AppendLineInvariant( this StringBuilder stringBuilder, FormattableString s )
            => stringBuilder.AppendLine( FormattableString.Invariant( s ) );
    }
}