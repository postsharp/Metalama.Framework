// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class UserExpression : IUserExpression
    {
        private readonly ExpressionSyntax _expression;
        private readonly SyntaxGenerationContext _generationContext;
        private readonly bool _isReferenceable;

        public UserExpression(
            ExpressionSyntax expression,
            IType type,
            SyntaxGenerationContext generationContext,
            bool isReferenceable = false,
            bool isAssignable = false )
        {
            this._expression = expression;
            this._generationContext = generationContext;
            this.Type = type;
            this.IsAssignable = isAssignable;
            this._isReferenceable = isReferenceable;
        }

        public RuntimeExpression ToRunTimeExpression() => new( this._expression, this.Type, this._generationContext, this._isReferenceable );

        public IType Type { get; }

        public bool IsAssignable { get; }

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}