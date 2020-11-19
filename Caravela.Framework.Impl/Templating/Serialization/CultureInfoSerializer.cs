using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    internal class CultureInfoSerializer : TypedObjectSerializer<CultureInfo>
    {
        public override ExpressionSyntax Serialize( CultureInfo o ) 
        {
            return ObjectCreationExpression(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName("System"),
                            IdentifierName("Globalization")),
                        IdentifierName("CultureInfo")))
                .WithNewKeyword(
                    Token(
                        TriviaList(),
                        SyntaxKind.NewKeyword,
                        TriviaList(
                            Space)))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]{
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(o.Name))),
                                Token(
                                    TriviaList(),
                                    SyntaxKind.CommaToken,
                                    TriviaList(
                                        Space)),
                                Argument(
                                    LiteralExpression(o.UseUserOverride ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression))})));
        }
    }
}