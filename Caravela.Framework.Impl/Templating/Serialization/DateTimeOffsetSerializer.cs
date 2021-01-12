using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    [Obfuscation( Exclude = true )]
    internal class DateTimeOffsetSerializer : TypedObjectSerializer<DateTimeOffset>
    {
        public override ExpressionSyntax Serialize( DateTimeOffset o )
        {
            string isoTime = o.ToString( "o" );
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