// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Metalama.Framework.Engine.Utilities.Roslyn;

internal static class IdentifierHelper
{
    public static bool IsIdentifierStartCharacter( char ch )
    {
        // identifier-start-character:
        //   letter-character
        //   _ (the underscore character U+005F)

        if ( ch < 'a' ) // '\u0061'
        {
            if ( ch < 'A' ) // '\u0041'
            {
                return false;
            }

            return ch <= 'Z'     // '\u005A'
                   || ch == '_'; // '\u005F'
        }

        if ( ch <= 'z' ) // '\u007A'
        {
            return true;
        }

        if ( ch <= '\u007F' ) // max ASCII
        {
            return false;
        }

        return IsLetterChar( CharUnicodeInfo.GetUnicodeCategory( ch ) );
    }

    /// <summary>
    /// Returns true if the Unicode character can be a part of an identifier.
    /// </summary>
    /// <param name="ch">The Unicode character.</param>
    public static bool IsIdentifierPartCharacter( char ch )
    {
        // identifier-part-character:
        //   letter-character
        //   decimal-digit-character
        //   connecting-character
        //   combining-character
        //   formatting-character

        if ( ch < 'a' ) // '\u0061'
        {
            if ( ch < 'A' ) // '\u0041'
            {
                return ch >= '0'     // '\u0030'
                       && ch <= '9'; // '\u0039'
            }

            return ch <= 'Z'     // '\u005A'
                   || ch == '_'; // '\u005F'
        }

        if ( ch <= 'z' ) // '\u007A'
        {
            return true;
        }

        if ( ch <= '\u007F' ) // max ASCII
        {
            return false;
        }

        var cat = CharUnicodeInfo.GetUnicodeCategory( ch );

        return IsLetterChar( cat )
               || IsDecimalDigitChar( cat )
               || IsConnectingChar( cat )
               || IsCombiningChar( cat )
               || IsFormattingChar( cat );
    }

    /// <summary>
    /// Check that the name is a valid Unicode identifier.
    /// </summary>
    public static bool IsValidIdentifier( [NotNullWhen( returnValue: true )] string? name )
    {
        if ( string.IsNullOrEmpty( name ) )
        {
            return false;
        }

        if ( !IsIdentifierStartCharacter( name[0] ) )
        {
            return false;
        }

        var nameLength = name.Length;

        for ( var i = 1; i < nameLength; i++ ) //NB: start at 1
        {
            if ( !IsIdentifierPartCharacter( name[i] ) )
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsLetterChar( UnicodeCategory cat )
    {
        // letter-character:
        //   A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nl 
        //   A Unicode-escape-sequence representing a character of classes Lu, Ll, Lt, Lm, Lo, or Nl

        switch ( cat )
        {
            case UnicodeCategory.UppercaseLetter:
            case UnicodeCategory.LowercaseLetter:
            case UnicodeCategory.TitlecaseLetter:
            case UnicodeCategory.ModifierLetter:
            case UnicodeCategory.OtherLetter:
            case UnicodeCategory.LetterNumber:
                return true;
        }

        return false;
    }

    private static bool IsCombiningChar( UnicodeCategory cat )
    {
        // combining-character:
        //   A Unicode character of classes Mn or Mc 
        //   A Unicode-escape-sequence representing a character of classes Mn or Mc

        switch ( cat )
        {
            case UnicodeCategory.NonSpacingMark:
            case UnicodeCategory.SpacingCombiningMark:
                return true;
        }

        return false;
    }

    private static bool IsDecimalDigitChar( UnicodeCategory cat )
    {
        // decimal-digit-character:
        //   A Unicode character of the class Nd 
        //   A unicode-escape-sequence representing a character of the class Nd

        return cat == UnicodeCategory.DecimalDigitNumber;
    }

    private static bool IsConnectingChar( UnicodeCategory cat )
    {
        // connecting-character:  
        //   A Unicode character of the class Pc
        //   A unicode-escape-sequence representing a character of the class Pc

        return cat == UnicodeCategory.ConnectorPunctuation;
    }

    /// <summary>
    /// Returns true if the Unicode character is a formatting character (Unicode class Cf).
    /// </summary>
    /// <param name="ch">The Unicode character.</param>
    internal static bool IsFormattingChar( char ch )
    {
        // There are no FormattingChars in ASCII range

        return ch > 127 && IsFormattingChar( CharUnicodeInfo.GetUnicodeCategory( ch ) );
    }

    /// <summary>
    /// Returns true if the Unicode character is a formatting character (Unicode class Cf).
    /// </summary>
    /// <param name="cat">The Unicode character.</param>
    private static bool IsFormattingChar( UnicodeCategory cat )
    {
        // formatting-character:  
        //   A Unicode character of the class Cf
        //   A unicode-escape-sequence representing a character of the class Cf

        return cat == UnicodeCategory.Format;
    }

    public static void ValidateSyntaxTreeName( string name )
    {
        for ( var i = 0; i < name.Length; i++ )
        {
            var c = name[i];

            if ( !IsIdentifierPartCharacter( c )
                 && c != '.'
                 && c != ','
                 && c != '-'
                 && c != '_'
                 && c != ' '
                 && c != '('
                 && c != ')'
                 && c != '['
                 && c != ']'
                 && c != '{'
                 && c != '}' )
            {
                throw new ArgumentException( $"The identifier '{name}' contains the character '{c}', which is not allowed." );
            }
        }
    }
}