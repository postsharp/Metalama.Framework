// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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