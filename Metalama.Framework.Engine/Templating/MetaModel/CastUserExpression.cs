// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;

namespace Metalama.Framework.Impl.Templating.MetaModel
{
    internal class CastUserExpression : IUserExpression
    {
        private readonly object? _value;

        public CastUserExpression( IType type, object? value )
        {
            this.Type = type;
            this._value = value;
        }

        public RuntimeExpression ToRunTimeExpression()
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;

            switch ( this._value )
            {
                case ExpressionSyntax expressionSyntax:
                    return new RuntimeExpression(
                        SyntaxFactory.ParenthesizedExpression(
                                syntaxGenerationContext.SyntaxGenerator.CastExpression( this.Type.GetSymbol(), expressionSyntax ) )
                            .WithAdditionalAnnotations( Simplifier.Annotation ),
                        this.Type,
                        syntaxGenerationContext );

                case RuntimeExpression runtimeExpression:

                    var syntax = SyntaxFactory.ParenthesizedExpression(
                            syntaxGenerationContext.SyntaxGenerator.CastExpression( this.Type.GetSymbol(), runtimeExpression.Syntax ) )
                        .WithAdditionalAnnotations( Simplifier.Annotation );

                    return new RuntimeExpression( syntax, this.Type, syntaxGenerationContext );

                default:
                    throw new AssertionFailedException();
            }
        }

        public bool IsAssignable => false;

        public IType Type { get; }

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}