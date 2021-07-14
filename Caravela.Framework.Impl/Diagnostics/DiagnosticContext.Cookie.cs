// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Diagnostics;
using System;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal static partial class DiagnosticContext
    {
        private class Cookie : IDisposable
        {
            private readonly IDiagnosticLocation? _oldLocation;

            public Cookie( IDiagnosticLocation? oldLocation )
            {
                this._oldLocation = oldLocation;
            }

            public void Dispose()
            {
                _current.Value = this._oldLocation;
            }
        }
    }
}