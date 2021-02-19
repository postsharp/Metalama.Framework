using System;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal abstract partial class DiagnosticSink
    {
        private class NonLocalizableString : LocalizableString
        {
            private readonly string _text;

            public NonLocalizableString( string text )
            {
                this._text = text;
            }

            protected override string GetText( IFormatProvider? formatProvider )
            {
                return this._text;
            }

            protected override int GetHash()
            {
                return this._text.GetHashCode();
            }

            protected override bool AreEqual( object? other )
            {
                return other is NonLocalizableString otherString && otherString._text.Equals( this._text, StringComparison.OrdinalIgnoreCase );
            }
        }
    }
}