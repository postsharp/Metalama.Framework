// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateAnnotator
    {
        private readonly struct ConditionalBranchCookie : IDisposable
        {
            private readonly TemplateAnnotator _parent;
            private readonly bool _initialValue;

            public ConditionalBranchCookie( TemplateAnnotator parent, bool initialValue )
            {
                this._parent = parent;
                this._initialValue = initialValue;
            }

            public void Dispose()
            {
                this._parent._isRuntimeConditionalBlock = this._initialValue;
            }
        }
    }
}