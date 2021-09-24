// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using System;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class CastDynamicExpression : IDynamicExpression
    {
        private readonly object? _value;

        public CastDynamicExpression( IType type, object? value )
        {
            this.Type = type;
            this._value = value;
        }

        public RuntimeExpression CreateExpression( string? expressionText = null, Location? location = null )
        {
            var expression = this._value switch
            {
                ExpressionSyntax expressionSyntax => expressionSyntax,
                RuntimeExpression runtimeExpression => runtimeExpression.Syntax,
                _ => throw new AssertionFailedException()
            };

            return new RuntimeExpression(
                SyntaxFactory.ParenthesizedExpression( SyntaxGeneratorFactory.DefaultSyntaxGenerator.CastExpression( this.Type.GetSymbol(), expression ) )
                    .WithAdditionalAnnotations( Simplifier.Annotation ),
                this.Type );
        }

        public bool IsAssignable => false;

        public IType Type { get; }

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}