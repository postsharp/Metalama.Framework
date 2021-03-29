using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caravela.Framework.Tests.Integration.Highlighting;
using Caravela.Framework.Tests.Integration.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.AspectWorkbench.Model
{
    internal class TestSerializer
    {
        private static string GetExpectedOutputFilePath( string testFilePath ) => Path.ChangeExtension( testFilePath, ".transformed.txt" );

        public async Task<TemplateTest> LoadFromFileAsync( string filePath )
        {
            var testName = Path.GetFileNameWithoutExtension( filePath );
            var testSource = await File.ReadAllTextAsync( filePath );

            var expectedOutputFilePath = GetExpectedOutputFilePath( filePath );
            string? expectedOutput = null;

            if ( File.Exists( expectedOutputFilePath ) )
            {
                expectedOutput = File.ReadAllText( expectedOutputFilePath );
            }

            return new TemplateTest
            {
                Input = new TestInput( testName, null, testSource, null ),
                ExpectedOutput = expectedOutput
            };
        }

        public async Task SaveToFileAsync( TemplateTest test, string filePath )
        {
            await File.WriteAllTextAsync( filePath, test.Input.TestSource.ToString() );

            var expectedOutputFilePath = GetExpectedOutputFilePath( filePath );
            await File.WriteAllTextAsync( expectedOutputFilePath, test.ExpectedOutput );
        }

        private static SyntaxNode[] GetFields( SyntaxNode syntaxRoot )
        {
            return syntaxRoot.DescendantNodes().Where( n => n.IsKind( SyntaxKind.FieldDeclaration ) ).ToArray();
        }

        private static SyntaxNode GetField( IEnumerable<SyntaxNode> fields, string fieldName )
        {
            return fields.FirstOrDefault(
                f => f.DescendantNodes().OfType<VariableDeclaratorSyntax>().Any( n => n.Identifier.Text.Equals( fieldName, StringComparison.Ordinal ) ) );
        }

        private static string? GetFieldValue( SyntaxNode fieldNode )
        {
            if ( fieldNode == null )
            {
                return null;
            }

            return fieldNode.DescendantNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault()?.Token.ValueText;
        }

        private static SyntaxNode SetFieldValue( SyntaxNode root, string fieldName, string? value )
        {
            return root.ReplaceNode(
                GetField( GetFields( root ), fieldName ).DescendantNodes().OfType<LiteralExpressionSyntax>().First(),
                value != null ?
                    LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( "@\"" + value.Replace( "\"", "\"\"" ) + "\"", value ) )
                    :
                    LiteralExpression( SyntaxKind.NullLiteralExpression ) );
        }
    }
}
