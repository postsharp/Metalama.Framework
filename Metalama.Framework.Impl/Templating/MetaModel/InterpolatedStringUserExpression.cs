// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace Metalama.Framework.Impl.Templating.MetaModel
{
    internal class InterpolatedStringUserExpression : IUserExpression
    {
        private readonly InterpolatedStringBuilder _builder;

        public InterpolatedStringUserExpression( InterpolatedStringBuilder builder, ICompilation compilation )
        {
            this._builder = builder;
            this.Type = compilation.TypeFactory.GetSpecialType( SpecialType.String );
        }

        public RuntimeExpression ToRunTimeExpression()
        {
            var syntaxGenerationContext = TemplateExpansionContext.CurrentSyntaxGenerationContext;
            List<InterpolatedStringContentSyntax> contents = new( this._builder.Items.Count );

            var textAccumulator = new StringBuilder();

            void FlushTextToken()
            {
                if ( textAccumulator.Length > 0 )
                {
                    var text = textAccumulator.ToString()
                        .Replace( "{", "{{" )
                        .Replace( "}", "}}" );

                    var literal = SyntaxFactory.Literal( text );
                    var escapedText = literal.Text.Substring( 1, literal.Text.Length - 2 );

                    contents.Add(
                        SyntaxFactory.InterpolatedStringText(
                            SyntaxFactory.Token( default, SyntaxKind.InterpolatedStringTextToken, escapedText, text, default ) ) );

                    textAccumulator.Length = 0;
                }
            }

            foreach ( var content in this._builder.Items )
            {
                switch ( content )
                {
                    case string text:
                        textAccumulator.Append( text );

                        break;

                    case InterpolatedStringBuilder.Token token:

                        FlushTextToken();

                        contents.Add(
                            SyntaxFactory.Interpolation(
                                RuntimeExpression.FromValue( token.Expression, this.Type.Compilation, syntaxGenerationContext )
                                    .Syntax ) );

                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }

            FlushTextToken();

            var syntax = TemplateSyntaxFactory.RenderInterpolatedString(
                SyntaxFactory.InterpolatedStringExpression(
                    SyntaxFactory.Token( SyntaxKind.InterpolatedStringStartToken ),
                    SyntaxFactory.List( contents ),
                    SyntaxFactory.Token( SyntaxKind.InterpolatedStringEndToken ) ) );

            return new RuntimeExpression( syntax, this.Type, syntaxGenerationContext );
        }

        public bool IsAssignable => false;

        public IType Type { get; set; }

        object? IExpression.Value { get => this; set => throw new NotSupportedException(); }
    }
}