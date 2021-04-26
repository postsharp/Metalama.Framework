// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateAnnotator
    {
        private readonly struct ForceBuildTimeExpressionCookie : IDisposable
        {
            private readonly TemplateAnnotator _parent;
            private readonly bool _initialValue;
            private readonly string? _initialReason;

            public ForceBuildTimeExpressionCookie( TemplateAnnotator parent )
            {
                this._parent = parent;
                this._initialValue = parent._forceCompileTimeOnlyExpression;
                this._initialReason = parent._forceCompileTimeOnlyExpressionReason;
            }

            public void Dispose()
            {
                this._parent._forceCompileTimeOnlyExpression = this._initialValue;
            }
        }
    }
}