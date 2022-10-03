// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Text;
using SpecialType = Metalama.Framework.Code.SpecialType;

namespace Metalama.Framework.Engine.Templating.Expressions
{
    internal class InterpolatedStringUserExpression : UserExpression
    {
        private readonly InterpolatedStringBuilder _builder;

        public InterpolatedStringUserExpression( InterpolatedStringBuilder builder, ICompilation compilation )
        {
            this._builder = builder;
            this.Type = compilation.GetCompilationModel().Factory.GetSpecialType( SpecialType.String );
        }

        protected override ExpressionSyntax ToSyntax( SyntaxGenerationContext syntaxGenerationContext )
        {
            List<InterpolatedStringContentSyntax> contents = new( this._builder.Items.Count );

            var textAccumulator = new StringBuilder();

            void FlushTextToken()
            {
                if ( textAccumulator.Length > 0 )
                {
                    var text = textAccumulator.ToString()
                        .ReplaceOrdinal( "{", "{{" )
                        .ReplaceOrdinal( "}", "}}" );

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

                        var tokenSyntax = TypedExpressionSyntax.FromValue( token.Expression, this.Type.Compilation, syntaxGenerationContext ).Syntax;

                        if ( tokenSyntax is LiteralExpressionSyntax literal && literal.Token.IsKind( SyntaxKind.StringLiteralToken ) )
                        {
                            textAccumulator.Append( literal.Token.Text.Substring( 1, literal.Token.Text.Length - 2 ) );
                        }
                        else
                        {
                            var interpolation = InterpolationSyntaxHelper.Fix( SyntaxFactory.Interpolation( tokenSyntax ) );
                            contents.Add( interpolation );
                        }

                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }

            FlushTextToken();

            return TemplateSyntaxFactory.RenderInterpolatedString(
                SyntaxFactory.InterpolatedStringExpression(
                    SyntaxFactory.Token( SyntaxKind.InterpolatedStringStartToken ),
                    SyntaxFactory.List( contents ),
                    SyntaxFactory.Token( SyntaxKind.InterpolatedStringEndToken ) ) );
        }

        public override IType Type { get; }
    }
}