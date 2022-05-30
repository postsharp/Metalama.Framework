// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    internal class CastUserExpression : IUserExpression
    {
        private readonly object? _value;

        public CastUserExpression( IType type, object? value )
        {
            this.Type = type;
            this._value = value;
        }

        public ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
        {
            var valueSyntax = this._value switch
            {
                ExpressionSyntax e => e,
                RunTimeTemplateExpression runtimeExpression => runtimeExpression.Syntax,
                IUserExpression ue => ue.ToSyntax( syntaxGenerationContext ),
                _ => throw new AssertionFailedException()
            };

            return SyntaxFactory.ParenthesizedExpression( syntaxGenerationContext.SyntaxGenerator.CastExpression( this.Type.GetSymbol(), valueSyntax ) )
                .WithAdditionalAnnotations( Simplifier.Annotation );
        }

        public RunTimeTemplateExpression ToRunTimeTemplateExpression( SyntaxGenerationContext syntaxGenerationContext )
        {
            return new RunTimeTemplateExpression(
                this.ToSyntax( syntaxGenerationContext ),
                this.Type,
                syntaxGenerationContext );
        }

        public bool IsAssignable => false;

        public IType Type { get; }

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}