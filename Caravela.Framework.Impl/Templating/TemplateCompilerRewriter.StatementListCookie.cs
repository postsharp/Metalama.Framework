using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    internal sealed partial class TemplateCompilerRewriter
    {
        private readonly struct StatementListCookie : IDisposable
        {
            private readonly TemplateCompilerRewriter _parent;
            private readonly string _initialVariableName;
            private readonly List<StatementSyntax> _initialList;

            public StatementListCookie(TemplateCompilerRewriter parent, string initialVariableName,
                List<StatementSyntax> initialList)
            {
                this._parent = parent;
                this._initialVariableName = initialVariableName;
                this._initialList = initialList;
            }

            public void Dispose()
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                //    Would be true in the default instance.
                
                if (this._parent != null)
                {
                    this._parent._currentStatementListVariableName = this._initialVariableName;
                    this._parent._currentMetaStatementList = this._initialList;
                }
            }
        }
    }
}