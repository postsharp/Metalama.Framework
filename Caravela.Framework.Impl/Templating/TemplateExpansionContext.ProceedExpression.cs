// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Templating
{
    internal partial class TemplateExpansionContext
    {
        private class ProceedExpression : IDynamicExpression
        {
            private readonly TemplateExpansionContext _parent;
            private readonly string _methodName;

            public ProceedExpression( string methodName, TemplateExpansionContext parent )
            {
                this._methodName = methodName;
                this._parent = parent;
            }

            public RuntimeExpression CreateExpression( string? expressionText = null, Location? location = null )
            {
                var targetMethod = this._parent.MetaApi.Target.Method;

                var isValid = this._methodName switch
                {
                    nameof(meta.Proceed) => true,
                    nameof(meta.ProceedAsync) => targetMethod.GetAsyncInfoImpl().IsAwaitable,
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
                    throw TemplatingDiagnosticDescriptors.CannotUseSpecificProceedInThisContext.CreateException(
                        location,
                        (this._methodName, targetMethod) );
                }

                return this._parent._proceedExpression!.CreateExpression( expressionText, location );
            }

            public bool IsAssignable => false;

            public IType Type => this._parent._proceedExpression!.Type;

            object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
        }
    }
}