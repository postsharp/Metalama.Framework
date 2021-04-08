// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    // ReSharper disable once UnusedTypeParameter
    internal partial class StrongDiagnosticDescriptor<T>
    {
        private class NonLocalizedString : LocalizableString
        {
            private readonly string _message;
            private readonly object[] _arguments;

            public NonLocalizedString( string message, object[] arguments )
            {
                this._message = message;
                this._arguments = arguments;
            }

            protected override string GetText( IFormatProvider? formatProvider )
                => string.Format( DiagnosticFormatter.Instance, this._message, this._arguments );

            protected override int GetHash() => this._message.GetHashCode();

            protected override bool AreEqual( object? other ) => ReferenceEquals( this, other );
        }
    }
}