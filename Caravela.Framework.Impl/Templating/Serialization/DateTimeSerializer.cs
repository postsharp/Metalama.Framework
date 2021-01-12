using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    [Obfuscation( Exclude = true )]
    internal class DateTimeSerializer : TypedObjectSerializer<DateTime>
    {
        public override ExpressionSyntax Serialize( DateTime o )
        {
            return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName( "System" ),
                            IdentifierName( "DateTime" ) ),
                        IdentifierName( "FromBinary" ) ) )
                .AddArgumentListArguments( 
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal( o.ToBinary() ) ) ) ) ;
        }
    }
}