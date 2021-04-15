// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System;

namespace Caravela.Framework.Impl.Diagnostics
{
    public partial class DiagnosticSink
    {
        private class RestoreLocationCookie : IDisposable
        {
            private readonly DiagnosticSink _parent;
            private readonly ICodeElement? _scope;

            public RestoreLocationCookie( DiagnosticSink parent, ICodeElement? scope )
            {
                this._parent = parent;
                this._scope = scope;
            }

            public void Dispose()
            {
                this._parent.DefaultScope = this._scope;
            }
        }
    }
}