// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating.Expressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Templating
{
    internal partial class TemplateExpansionContext
    {
        private class ProceedUserExpression : UserExpression
        {
            private readonly TemplateExpansionContext _parent;
            private readonly string _methodName;

            public ProceedUserExpression( string methodName, TemplateExpansionContext parent )
            {
                this._methodName = methodName;
                this._parent = parent;
            }

            public override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
            {
                this.Validate();

                return this._parent._proceedExpression!.ToSyntax( syntaxGenerationContext );
            }

            public override RunTimeTemplateExpression ToRunTimeTemplateExpression( SyntaxGenerationContext syntaxGenerationContext )
            {
                this.Validate();

                return this._parent._proceedExpression!.ToRunTimeTemplateExpression( syntaxGenerationContext );
            }

            private void Validate()
            {
                var targetMethod = this._parent.MetaApi.Target.Method;

                var isValid = this._methodName switch
                {
                    nameof(meta.Proceed) => true,
                    nameof(meta.ProceedAsync) => targetMethod.GetAsyncInfoImpl().IsAwaitableOrVoid,
                    nameof(meta.ProceedEnumerable) => targetMethod.GetIteratorInfoImpl().EnumerableKind is EnumerableKind.IEnumerable or EnumerableKind
                        .UntypedIEnumerable,
                    nameof(meta.ProceedEnumerator) => targetMethod.GetIteratorInfoImpl().EnumerableKind is EnumerableKind.IEnumerator or EnumerableKind
                        .UntypedIEnumerator,
                    "ProceedAsyncEnumerable" => targetMethod.GetIteratorInfoImpl().EnumerableKind is EnumerableKind.IAsyncEnumerable,
                    "ProceedAsyncEnumerator" => targetMethod.GetIteratorInfoImpl().EnumerableKind is EnumerableKind.IAsyncEnumerator,
                    _ => throw new ArgumentOutOfRangeException()
                };

                if ( !isValid )
                {
                    throw TemplatingDiagnosticDescriptors.CannotUseSpecificProceedInThisContext.CreateException( (this._methodName, targetMethod) );
                }
            }

            public override IType Type => this._parent._proceedExpression!.Type;
        }
    }
}