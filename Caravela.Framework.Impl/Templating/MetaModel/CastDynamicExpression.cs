// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class CastDynamicExpression : IDynamicExpression
    {
        private readonly IType _type;
        private readonly object? _value;

        public CastDynamicExpression( IType type, object? value )
        {
            this._type = type;
            this._value = value;
        }

        public RuntimeExpression? CreateExpression( string? expressionText = null, Location? location = null )
        {
            var expression = this._value switch
            {
                null => SyntaxFactoryEx.Null,
                ExpressionSyntax expressionSyntax => expressionSyntax,
                RuntimeExpression runtimeExpression => runtimeExpression.Syntax,
                _ => SyntaxFactoryEx.LiteralExpression( this._value )
            };

            return new RuntimeExpression(
                SyntaxFactory.ParenthesizedExpression( LanguageServiceFactory.CSharpSyntaxGenerator.CastExpression( this._type.GetSymbol(), expression ) )
                    .WithAdditionalAnnotations( Simplifier.Annotation ) );
        }
    }
}