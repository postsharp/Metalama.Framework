// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    internal class DefaultUserExpression : IUserExpression
    {
        public DefaultUserExpression( IType type )
        {
            this.Type = type;
        }

        public ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
        {
            var typeSymbol = this.Type.GetSymbol();
            var expression = syntaxGenerationContext.SyntaxGenerator.DefaultExpression( typeSymbol );

            if ( expression is not DefaultExpressionSyntax )
            {
                // We need to specify the type explicitly to preserve the typing.
                expression = syntaxGenerationContext.SyntaxGenerator.CastExpression(
                    typeSymbol.IsReferenceType ? typeSymbol.WithNullableAnnotation( NullableAnnotation.Annotated ) : typeSymbol,
                    expression );
            }

            return expression;
        }

        public RunTimeTemplateExpression ToRunTimeTemplateExpression( SyntaxGenerationContext syntaxGenerationContext )
        {
            return new RunTimeTemplateExpression(
                this.ToSyntax( syntaxGenerationContext ),
                this.Type,
                syntaxGenerationContext );
        }

        public IType Type { get; }

        bool IExpression.IsAssignable => false;

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}