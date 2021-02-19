using System;
using Caravela.Framework.Diagnostics;

namespace Caravela.Framework.Impl.Diagnostics
{
    internal abstract partial class DiagnosticSink
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