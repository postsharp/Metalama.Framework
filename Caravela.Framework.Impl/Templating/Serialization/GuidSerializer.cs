using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class GuidSerializer : TypedObjectSerializer<Guid>
    {
        public override ExpressionSyntax Serialize( Guid o )
        {
            byte[] b = o.ToByteArray();
            
            int a = ((int)b[3] << 24) | ((int)b[2] << 16) | ((int)b[1] << 8) | b[0];
            short b2 = (short)(((int)b[5] << 8) | b[4]);
            short c = (short)(((int)b[7] << 8) | b[6]);
            byte d = b[8];
            byte e = b[9];
            byte f = b[10];
            byte g = b[11];
            byte h = b[12];
            byte i = b[13];
            byte j = b[14];
            byte k = b[15];
            return ObjectCreationExpression(
                    QualifiedName(
                        IdentifierName( "System" ),
                        IdentifierName( "Guid" ) ) )
                .AddArgumentListArguments(
                    Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( a ) ) ),
                    Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( b2 ) ) ),
                    Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( c ) ) ),
                    Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( d ) ) ),
                    Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( e ) ) ),
                    Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( f ) ) ),
                    Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( g ) ) ),
                    Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( h ) ) ),
                    Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( i ) ) ),
                    Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( j ) ) ),
                    Argument( LiteralExpression( SyntaxKind.NumericLiteralExpression, Literal( k ) ) )
                ).NormalizeWhitespace();
        }
    }
}