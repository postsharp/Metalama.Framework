using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Caravela.TestFramework.Templating;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.Text;
using Caravela.TestFramework;

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
            string testCategoryName = Path.GetFileName( Path.GetDirectoryName( filePath ) );

            if ( test.OriginalSyntaxRoot == null )
            {
                // This is a new test without a source file.
                string testSource = string.Format( NewTestDefaults.EmptyUnitTest, testCategoryName, testName );
                test.OriginalSyntaxRoot = await CSharpSyntaxTree.ParseText( testSource, encoding: Encoding.UTF8 ).GetRootAsync();
            }

            // Make sure that the main source file of the test category exists.
            // The main category file path is 'Caravela.Templating.UnitTests\{CATEGORY}Tests.cs'.
            // The file path of each test within the category is 'Caravela.Templating.UnitTests\{CATEGORY}\{TEST}.cs'.
            string testCategorySourcePath = Path.Combine( Path.GetDirectoryName( Path.GetDirectoryName( filePath ) ), $"{testCategoryName}Tests.cs" );
            if ( !File.Exists( testCategorySourcePath ) )
            {
                string testCategorySource = string.Format( NewTestDefaults.TestCategoryMainSource, testCategoryName );
                File.WriteAllText( testCategorySourcePath, testCategorySource );
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
            if ( fieldNode == null )
            {
                return null;
            }
            
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
