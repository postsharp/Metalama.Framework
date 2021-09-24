// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class DefaultDynamicExpression : IDynamicExpression
    {
        public DefaultDynamicExpression( IType type )
        {
            this.Type = type;
        }

        public RuntimeExpression CreateExpression( string? expressionText = null, Location? location = null )
        {
            var typeSymbol = this.Type.GetSymbol();
            var expression = SyntaxGeneratorFactory.DefaultSyntaxGenerator.DefaultExpression( typeSymbol );

            if ( expression is not DefaultExpressionSyntax )
            {
                // We need to specify the type explicitly to preserve the typing.
                expression = SyntaxGeneratorFactory.DefaultSyntaxGenerator.CastExpression(
                    typeSymbol.IsReferenceType ? typeSymbol.WithNullableAnnotation( NullableAnnotation.Annotated ) : typeSymbol,
                    expression );
            }

            return new RuntimeExpression( expression, this.Type );
        }

        public IType Type { get; }

        bool IExpression.IsAssignable => false;

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}