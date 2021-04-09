// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.Templating
{
    internal sealed partial class TemplateCompilerRewriter
    {
        private readonly struct StatementListCookie : IDisposable
        {
            private readonly TemplateCompilerRewriter _parent;
            private readonly MetaContext? _previousMetaContext;

            public StatementListCookie( TemplateCompilerRewriter parent, MetaContext? previousMetaContext )
            {
                this._parent = parent;
                this._previousMetaContext = previousMetaContext;
            }

            public void Dispose()
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                //    Would be true in the default instance.

                if ( this._parent != null )
                {
                    this._parent._currentMetaContext = this._previousMetaContext;
                }
            }
        }
    }
}