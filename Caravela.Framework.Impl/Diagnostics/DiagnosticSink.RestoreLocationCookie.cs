// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Impl.Diagnostics
{
    public abstract partial class DiagnosticSink
    {
        private class RestoreLocationCookie : IDisposable
        {
            private readonly DiagnosticSink _parent;
            private readonly IDiagnosticLocation? _oldLocation;

            public RestoreLocationCookie( DiagnosticSink parent, IDiagnosticLocation? oldLocation )
            {
                this._parent = parent;
                this._oldLocation = oldLocation;
            }

            public void Dispose()
            {
                this._parent.DefaultLocation = this._oldLocation;
            }
        }
    }
}