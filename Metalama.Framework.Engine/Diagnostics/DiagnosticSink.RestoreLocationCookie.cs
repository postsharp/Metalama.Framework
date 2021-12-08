// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Impl.Diagnostics
{
    internal partial class UserDiagnosticSink
    {
        private class RestoreLocationCookie : IDisposable
        {
            private readonly UserDiagnosticSink _parent;
            private readonly IDeclaration? _scope;

            public RestoreLocationCookie( UserDiagnosticSink parent, IDeclaration? scope )
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