using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.TestFramework.Templating;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Text;

namespace Caravela.AspectWorkbench.Model
{
    class TestSerializer
    {
        public async Task<TemplateTest> LoadFromFileAsync( string filePath )
        {
            string testName = Path.GetFileNameWithoutExtension( filePath );
            string templateFieldName = $"{testName}_Template";
            string expectedOutputFieldName = $"{testName}_ExpectedOutput";
            string targetFieldName = $"{testName}_Target";

            string testSource = await File.ReadAllTextAsync( filePath );
            var syntaxTree = CSharpSyntaxTree.ParseText( testSource, encoding: Encoding.UTF8 );

            var syntaxRoot = await syntaxTree.GetRootAsync();

            var fields = GetFields( syntaxRoot );

            var templateSource = GetFieldValue( GetField( fields, templateFieldName ) );
            var expectedOutput = GetFieldValue( GetField( fields, expectedOutputFieldName ) );
            var targetSource = GetFieldValue( GetField( fields, targetFieldName ) );

            return new TemplateTest
            {
                OriginalSyntaxRoot = syntaxRoot,
                Input = new TestInput( templateSource, targetSource ),
                ExpectedOutput = expectedOutput
            };
        }

        public async Task SaveToFileAsync( TemplateTest test, string filePath )
        {
            string testName = Path.GetFileNameWithoutExtension( filePath );

            if ( test.OriginalSyntaxRoot == null )
            {
                string folderName = Path.GetFileName( Path.GetDirectoryName( filePath ) );
                string testSource = string.Format( NewTestDefaults.EmptyUnitTest, folderName, testName );
                test.OriginalSyntaxRoot = await CSharpSyntaxTree.ParseText( testSource, encoding: Encoding.UTF8 ).GetRootAsync();
            }

            string templateFieldName = $"{testName}_Template";
            string expectedOutputFieldName = $"{testName}_ExpectedOutput";
            string targetFieldName = $"{testName}_Target";

            var newRoot = test.OriginalSyntaxRoot;
            newRoot = SetFieldValue( newRoot, templateFieldName, test.Input.TemplateSource );
            newRoot = SetFieldValue( newRoot, expectedOutputFieldName, test.ExpectedOutput );
            newRoot = SetFieldValue( newRoot, targetFieldName, test.Input.TargetSource );

            await File.WriteAllTextAsync( filePath, newRoot.GetText().ToString() );
        }

        private static SyntaxNode[] GetFields( SyntaxNode syntaxRoot )
        {
            return syntaxRoot.DescendantNodes().Where( n => n.IsKind( SyntaxKind.FieldDeclaration ) ).ToArray();
        }

        private static SyntaxNode GetField( IEnumerable<SyntaxNode> fields, string fieldName )
        {
            return fields.FirstOrDefault(
                f => f.DescendantNodes().OfType<VariableDeclaratorSyntax>().Any( n => n.Identifier.Text.Equals( fieldName ) )
            );
        }

        private static string GetFieldValue( SyntaxNode fieldNode )
        {
            return fieldNode.DescendantNodes().OfType<LiteralExpressionSyntax>().FirstOrDefault()?.Token.ValueText;
        }

        private static SyntaxNode SetFieldValue( SyntaxNode root, string fieldName, string value )
        {
            return root.ReplaceNode(
                GetField( GetFields( root ), fieldName ).DescendantNodes().OfType<LiteralExpressionSyntax>().First(),
                LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( "@\"" + value.Replace( "\"", "\"\"" ) + "\"", value ) )
            );
        }
    }
}
