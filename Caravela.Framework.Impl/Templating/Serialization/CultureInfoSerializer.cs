using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Templating.Serialization
{
    [Obfuscation( Exclude = true )]
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
                .AddArgumentListArguments(  
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(o.Name))),
                                Argument(
                                    LiteralExpression(o.UseUserOverride ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression)))
                .NormalizeWhitespace(  );
        }
    }
}