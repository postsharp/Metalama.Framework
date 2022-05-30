// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    /// <summary>
    /// An implementation of <see cref="IUserExpression"/> where the syntax is known upfront.
    /// </summary>
    internal class UserExpression : IUserExpression
    {
        private readonly ExpressionSyntax _expression;
        private readonly bool _isReferenceable;

        public UserExpression(
            ExpressionSyntax expression,
            IType type,
            bool isReferenceable = false,
            bool isAssignable = false )
        {
            this._expression = expression;
            this.Type = type;
            this.IsAssignable = isAssignable;
            this._isReferenceable = isReferenceable;
        }

        public ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext ) => this._expression;

        public RunTimeTemplateExpression ToRunTimeTemplateExpression( SyntaxGenerationContext syntaxGenerationContext )
            => new( this._expression, this.Type, syntaxGenerationContext, this._isReferenceable );

        public IType Type { get; }

        public bool IsAssignable { get; }

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}