// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Diagnostics
{
    public abstract partial class DiagnosticSink
    {
        private class NonLocalizableString : LocalizableString
        {
            private readonly string _text;
            private readonly object[]? _args;

            public NonLocalizableString( string text, object[]? args )
            {
                this._text = text;
                this._args = args;
            }

            protected override string GetText( IFormatProvider? formatProvider )
            {
                if ( this._args == null || this._args.Length == 0 )
                {
                    return this._text;
                }

                return string.Format( formatProvider, this._text, formatProvider );
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