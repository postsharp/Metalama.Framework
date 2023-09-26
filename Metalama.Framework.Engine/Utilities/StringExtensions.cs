// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Metalama.Framework.Engine.Utilities
{
    [PublicAPI]
    public static class StringExtensions
    {
        internal static string TrimSuffix( this string s, string suffix )
            => s.EndsWith( suffix, StringComparison.Ordinal ) ? s.Substring( 0, s.Length - suffix.Length ) : s;

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

        public static string NotNull( this string? s ) => s.AssertNotNull( s );

        public static void AppendLineInvariant( this StringBuilder stringBuilder, FormattableString s )
            => stringBuilder.AppendLine( FormattableString.Invariant( s ) );

        public static void AppendInvariant( this StringBuilder stringBuilder, FormattableString s ) => stringBuilder.Append( FormattableString.Invariant( s ) );

        public static int GetHashCodeOrdinal( this string s )
#if NET5_0_OR_GREATER
            => s.GetHashCode( StringComparison.Ordinal );
#else
            => s.GetHashCode();
#endif

        public static string ToCamelCase( this string s )
        {
            var firstLetter = s[..1];

            return firstLetter.ToLowerInvariant() + (s.Length > 1 ? s[1..] : "");
        }

        private static readonly Regex _invalidIdentifierCharacters = new( "[^_0-9a-zA-Z]", RegexOptions.Compiled );

        private static readonly Regex _identifierRegex = new(
            @"^[_\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}][\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]*$",
            RegexOptions.Compiled );

        public static bool IsValidIdentifier( this string? identifier ) => identifier == null || _identifierRegex.IsMatch( identifier );

        public static string ToIdentifier( this string s )
        {
            s = _invalidIdentifierCharacters.Replace( s, string.Empty );

            if ( s == string.Empty || !(char.IsLetter( s[0] ) || s[0] == '_') )
            {
                s = '_' + s;
            }

            return s;
        }
    }
}