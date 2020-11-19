using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class GuidSerializer : TypedObjectSerializer<Guid>
    {
        public override ExpressionSyntax Serialize( Guid o )
        {
            List<SyntaxNodeOrToken> tokens = new List<SyntaxNodeOrToken>();
            bool first = true;
            foreach ( byte b in o.ToByteArray() )
            {
                if ( first )
                {
                    first = false;
                }
                else
                {
                    tokens.Add( Token( SyntaxKind.CommaToken ) );
                }

                tokens.Add( LiteralExpression(
                    SyntaxKind.NumericLiteralExpression,
                    Literal( b ) ) );
            }

            SyntaxNodeOrToken[] syntaxNodeOrTokens = tokens.ToArray();
            return ObjectCreationExpression(
                    QualifiedName(
                        IdentifierName( "System" ),
                        IdentifierName( "Guid" ) ) )
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList<ArgumentSyntax>(
                            Argument(
                                ArrayCreationExpression(
                                        ArrayType(
                                                PredefinedType(
                                                    Token( SyntaxKind.ByteKeyword ) ) )
                                            .WithRankSpecifiers(
                                                SingletonList<ArrayRankSpecifierSyntax>(
                                                    ArrayRankSpecifier(
                                                        SingletonSeparatedList<ExpressionSyntax>(
                                                            OmittedArraySizeExpression() ) )
                                                ) ) )
                                    .WithInitializer(
                                        InitializerExpression(
                                            SyntaxKind.ArrayInitializerExpression,
                                            SeparatedList<ExpressionSyntax>(
                                                syntaxNodeOrTokens ) )
                                    ) ) ) ) ).NormalizeWhitespace();
        }
    }
}