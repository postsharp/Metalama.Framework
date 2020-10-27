using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PostSharp.Caravela.AspectWorkbench
{
    public sealed partial class TemplateCompiler
    {
        private readonly struct StatementListCookie : IDisposable
        {
            private readonly TemplateCompiler _parent;
            private readonly string _initialVariableName;
            private readonly List<StatementSyntax> _initialList;

            public StatementListCookie(TemplateCompiler parent, string initialVariableName,
                List<StatementSyntax> initialList)
            {
                this._parent = parent;
                this._initialVariableName = initialVariableName;
                this._initialList = initialList;
            }

            public void Dispose()
            {
                if (this._parent != null)
                {
                    this._parent._currentStatementListVariableName = this._initialVariableName;
                    this._parent._currentMetaStatementList = this._initialList;
                }
            }
        }
    }
}