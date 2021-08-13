// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Diagnostics
{
    // Public because of TryCaravela.
    public sealed class NonLocalizedString : LocalizableString
    {
        private readonly string _message;
        private readonly object[] _arguments;

        public NonLocalizedString( string message, object[]? arguments = null )
        {
            this._message = message;
            this._arguments = arguments ?? Array.Empty<object>();
        }

        protected override string GetText( IFormatProvider? formatProvider )
            => this._arguments.Length == 0
                ? this._message
                : string.Format( DiagnosticFormatter.Instance, this._message, this._arguments );

        protected override int GetHash() => this._message.GetHashCode();

        protected override bool AreEqual( object? other ) => ReferenceEquals( this, other );
    }
}