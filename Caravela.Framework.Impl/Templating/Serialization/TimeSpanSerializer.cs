using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    [Obfuscation( Exclude = true )]
    internal class TimeSpanSerializer : TypedObjectSerializer<TimeSpan>
    {
        public override ExpressionSyntax Serialize( TimeSpan o )
        {
            return ObjectCreationExpression(
                    QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("TimeSpan")))
                .AddArgumentListArguments( 
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal(o.Ticks))))
                .NormalizeWhitespace(  );
        }
    }
}