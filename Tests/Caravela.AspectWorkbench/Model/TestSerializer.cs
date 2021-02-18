using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.AspectWorkbench.Model
{
    internal class TestSerializer
    {
        public async Task<TemplateTest> LoadFromFileAsync( string filePath )
        {
            var testName = Path.GetFileNameWithoutExtension( filePath );
            var templateFieldName = $"{testName}_Template";
            var expectedOutputFieldName = $"{testName}_ExpectedOutput";
            var targetFieldName = $"{testName}_Target";

            var testSource = await File.ReadAllTextAsync( filePath );
            var syntaxTree = CSharpSyntaxTree.ParseText( testSource, encoding: Encoding.UTF8 );

            var syntaxRoot = await syntaxTree.GetRootAsync();

            var fields = GetFields( syntaxRoot );

            var templateSource = GetFieldValue( GetField( fields, templateFieldName ) );
            var expectedOutput = GetFieldValue( GetField( fields, expectedOutputFieldName ) );
            var targetSource = GetFieldValue( GetField( fields, targetFieldName ) );

            return new TemplateTest
            {
                OriginalSyntaxRoot = syntaxRoot,
                Input = new TestInput( "interactive", templateSource, targetSource ),
                ExpectedOutput = expectedOutput
            };
        }

        public async Task SaveToFileAsync( TemplateTest test, string filePath )
        {
            var testName = Path.GetFileNameWithoutExtension( filePath );
            var testCategoryName = Path.GetFileName( Path.GetDirectoryName( filePath ) );

            if ( test.OriginalSyntaxRoot == null )
            {
                // This is a new test without a source file.
                var testSource = string.Format( NewTestDefaults.EmptyUnitTest, testCategoryName, testName );
                test.OriginalSyntaxRoot = await CSharpSyntaxTree.ParseText( testSource, encoding: Encoding.UTF8 ).GetRootAsync();
            }

            // Make sure that the main source file of the test category exists.
            // The main category file path is 'Caravela.Templating.UnitTests\{CATEGORY}Tests.cs'.
            // The file path of each test within the category is 'Caravela.Templating.UnitTests\{CATEGORY}\{TEST}.cs'.
            var parentDirectory1 = Path.GetDirectoryName( filePath );
            if ( parentDirectory1 == null )
            {
                throw new ArgumentOutOfRangeException( nameof( filePath ) );
            }

            var parentDirectory2 = Path.GetDirectoryName( parentDirectory1 );
            if ( parentDirectory2 == null )
            {
                throw new ArgumentOutOfRangeException( nameof( filePath ) );
            }

            var testCategorySourcePath = Path.Combine( parentDirectory2, $"{testCategoryName}Tests.cs" );
            if ( !File.Exists( testCategorySourcePath ) )
            {
                var testCategorySource = string.Format( NewTestDefaults.TestCategoryMainSource, testCategoryName );
                File.WriteAllText( testCategorySourcePath, testCategorySource );
            }

            var templateFieldName = $"{testName}_Template";
            var expectedOutputFieldName = $"{testName}_ExpectedOutput";
            var targetFieldName = $"{testName}_Target";

            var newRoot = test.OriginalSyntaxRoot;
            newRoot = SetFieldValue( newRoot, templateFieldName, test.Input?.TemplateSource );
            newRoot = SetFieldValue( newRoot, expectedOutputFieldName, test.ExpectedOutput );
            newRoot = SetFieldValue( newRoot, targetFieldName, test.Input?.TargetSource );

            await File.WriteAllTextAsync( filePath, newRoot.GetText().ToString() );
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
