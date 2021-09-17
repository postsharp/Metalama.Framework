// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.ExpressionBuilders;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using SpecialType = Caravela.Framework.Code.SpecialType;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    internal class InterpolatedStringDynamicExpression : IDynamicExpression
    {
        private readonly InterpolatedStringBuilder _builder;

        public InterpolatedStringDynamicExpression( InterpolatedStringBuilder builder, ICompilation compilation )
        {
            this._builder = builder;
            this.Type = compilation.TypeFactory.GetSpecialType( SpecialType.String );
        }

        public RuntimeExpression CreateExpression( string? expressionText = null, Location? location = null )
        {
            List<InterpolatedStringContentSyntax> contents = new( this._builder.Items.Count );

            foreach ( var content in this._builder.Items )
            {
                switch ( content )
                {
                    case string text:
                        contents.Add(
                            SyntaxFactory.InterpolatedStringText(
                                SyntaxFactory.Token( default, SyntaxKind.InterpolatedStringTextToken, text, text, default ) ) );

                        break;

                    case InterpolatedStringBuilder.Token token:
                        contents.Add( SyntaxFactory.Interpolation( RuntimeExpression.FromValue( token.Expression, this.Type.Compilation ).Syntax ) );

                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }

            var expression = TemplateSyntaxFactory.RenderInterpolatedString(
                SyntaxFactory.InterpolatedStringExpression(
                    SyntaxFactory.Token( SyntaxKind.InterpolatedStringStartToken ),
                    SyntaxFactory.List( contents ),
                    SyntaxFactory.Token( SyntaxKind.InterpolatedStringEndToken ) ) );

            return new RuntimeExpression( expression, this.Type );
        }

        public bool IsAssignable => false;

        public IType Type { get; set; }

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}