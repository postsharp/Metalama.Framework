// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Impl.CompileTime;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateAnnotator
    {
        private readonly struct BreakOrContinueScopeCookie : IDisposable
        {
            private readonly TemplateAnnotator _parent;
            private readonly SymbolDeclarationScope _initialValue;

            public BreakOrContinueScopeCookie( TemplateAnnotator parent, SymbolDeclarationScope initialValue )
            {
                this._parent = parent;
                this._initialValue = initialValue;
            }

            public void Dispose()
            {
                this._parent._breakOrContinueScope = this._initialValue;
            }
        }
    }
}