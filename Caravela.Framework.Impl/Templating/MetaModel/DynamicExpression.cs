// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class DynamicExpression : IDynamicExpression
    {
        private readonly ExpressionSyntax _expression;
        private readonly bool _isReferenceable;

        public DynamicExpression( ExpressionSyntax expression, IType type, bool isReferenceable = false, bool isAssignable = false )
        {
            this._expression = expression;
            this.Type = type;
            this.IsAssignable = isAssignable;
            this._isReferenceable = isReferenceable;
        }

        public RuntimeExpression CreateExpression( string? expressionText, Location? location = null )
            => new( this._expression, this.Type, this._isReferenceable );

        public IType Type { get; }

        public bool IsAssignable { get; }

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}