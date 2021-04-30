// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using System;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateAnnotator
    {
        private readonly struct ScopeContextCookie : IDisposable
        {
            private readonly TemplateAnnotator _parent;
            private readonly ScopeContext _initialValue;

            public ScopeContextCookie( TemplateAnnotator parent, ScopeContext initialValue)
            {
                this._parent = parent;
                this._initialValue = initialValue;
            }

            public void Dispose()
            {
                this._parent._currentScopeContext = this._initialValue;
            }
        }
    }
}
