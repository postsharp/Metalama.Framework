using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Serialization
{
    internal class DateTimeOffsetSerializer : TypedObjectSerializer<DateTimeOffset>
    {
        public override ExpressionSyntax Serialize( DateTimeOffset o )
        {
            var isoTime = o.ToString( "o" );
            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName( "System" ),
                            IdentifierName( "DateTimeOffset" ) ),
                        IdentifierName( "Parse" ) ) )
                .AddArgumentListArguments(
                    Argument(
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal( isoTime ) ) ) )
                .NormalizeWhitespace();
        }
    }
}