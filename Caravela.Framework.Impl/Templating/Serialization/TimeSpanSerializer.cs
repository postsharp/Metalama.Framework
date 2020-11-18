using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    public class TimeSpanSerializer : TypedObjectSerializer<TimeSpan>
    {
        public override ExpressionSyntax Serialize( TimeSpan o )
        {
            return ObjectCreationExpression(
                    QualifiedName(
                        IdentifierName("System"),
                        IdentifierName("TimeSpan")))
                .WithNewKeyword(
                    Token(
                        TriviaList(),
                        SyntaxKind.NewKeyword,
                        TriviaList(
                            Space)))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList<ArgumentSyntax>(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.NumericLiteralExpression,
                                    Literal(o.Ticks))))));
        }
    }
}