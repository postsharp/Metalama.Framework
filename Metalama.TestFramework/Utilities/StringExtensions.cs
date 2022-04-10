// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Text;

namespace Metalama.TestFramework.Utilities
{
    /// <summary>
    /// Extension methods to <see cref="string"/> to cope with the absence of a few methods in .NET Framework and
    /// the warnings we get when we don't use them in .NET 5.
    /// </summary>
    public static class StringExtensions
    {
        public static string ReplaceOrdinal( this string s, string oldValue, string newValue )
#if NET5_0_OR_GREATER
            => s.Replace( oldValue, newValue, StringComparison.Ordinal );
#else
            => s.Replace( oldValue, newValue );
#endif

        public static bool ContainsOrdinal( this string s, string substring )
#if NET5_0_OR_GREATER
            => s.Contains( substring, StringComparison.Ordinal );
#else
            => s.Contains( substring );
#endif

        public static bool ContainsOrdinal( this string s, char c )
#if NET5_0_OR_GREATER
            => s.Contains( c, StringComparison.Ordinal );
#else
            => s.IndexOf( c ) >= 0;
#endif

        public static int IndexOfOrdinal( this string s, char c )
#if NET5_0_OR_GREATER
            => s.IndexOf( c, StringComparison.Ordinal );
#else
            => s.IndexOf( c );
#endif

        public static string NotNull( this string? s ) => s!;
        
        public static void AppendLineInvariant( this StringBuilder stringBuilder, FormattableString s )
            => stringBuilder.AppendLine( FormattableString.Invariant( s ) );
    }
}