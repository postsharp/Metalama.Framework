using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.TestFramework.Templating.Highlighting
{
    internal class HighlightingUnitTestRunner : TemplateTestRunnerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightingUnitTestRunner"/> class.
        /// </summary>
        public HighlightingUnitTestRunner()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HighlightingUnitTestRunner"/> class.
        /// </summary>
        /// <param name="testAnalyzers">A list of analyzers to invoke on the test source.</param>
        public HighlightingUnitTestRunner( IEnumerable<CSharpSyntaxVisitor> testAnalyzers )
            : base( testAnalyzers )
        {
        }

        public override async Task<TestResult> RunAsync( TestInput testInput )
        {
            var result = await base.RunAsync( testInput );

            if ( !result.Success )
            {
                return result;
            }

            result.Success = false;

            var highlightedTemplateDirectory = Path.Combine(
                testInput.ProjectDirectory,
                "obj",
                "highlighted",
                Path.GetDirectoryName( testInput.TestSourcePath ) ?? "" );

            var highlightedTemplatePath = Path.Combine(
                highlightedTemplateDirectory,
                Path.GetFileNameWithoutExtension( testInput.TestSourcePath ) + ".highlighted.html" );

            Directory.CreateDirectory( Path.GetDirectoryName( highlightedTemplatePath ) );

            var sourceText = await result.TemplateDocument.GetTextAsync();
            //TODO: Don't use the obsolete constructor
            var classifier = new TextSpanClassifier( sourceText, true );
            classifier.Visit( result.AnnotatedTemplateSyntax );

            using ( var textWriter = new StreamWriter( highlightedTemplatePath, false, Encoding.UTF8 ) )
            {
                textWriter.WriteLine( "<html>" );
                textWriter.WriteLine( "<head>" );

                void WriteStyleSheetPaths( [CallerFilePath] string callingSourceFilePath = null )
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    var styleSheetPath = Path.Combine( Path.GetDirectoryName( callingSourceFilePath ), "highlighting.css" );
#pragma warning restore CS8604 // Possible null reference argument.
                    var styleSheetPathRelativeToActualHtmlFile = Path.GetRelativePath( highlightedTemplateDirectory, styleSheetPath );

                    var testSourceAbsolutePath = Path.Combine( testInput.ProjectDirectory, testInput.TestSourcePath );
                    var styleSheetPathRelativeToExpectedHtmlFile = Path.GetRelativePath( testSourceAbsolutePath, styleSheetPath );

                    textWriter.WriteLine( $"<!-- Just one of these paths is supposed to work. -->" );
                    textWriter.WriteLine( $"<link rel='stylesheet' href='{styleSheetPathRelativeToActualHtmlFile}' />" );
                    textWriter.WriteLine( $"<link rel='stylesheet' href='{styleSheetPathRelativeToExpectedHtmlFile}' />" );
                }

                WriteStyleSheetPaths();

                textWriter.WriteLine( "</head>" );
                textWriter.WriteLine( "<body><pre>" );

                var i = 0;

                foreach ( var classifiedSpan in classifier.ClassifiedTextSpans )
                {
                    if ( i < classifiedSpan.Span.Start )
                    {
                        textWriter.Write( sourceText.GetSubText( new TextSpan( i, classifiedSpan.Span.Start - i ) ) );
                    }

                    textWriter.Write( $"<span class='{classifiedSpan.Classification}'>" + sourceText.GetSubText( classifiedSpan.Span ) + "</span>" );

                    i = classifiedSpan.Span.End;
                }

                if ( i < sourceText.Length )
                {
                    textWriter.Write( sourceText.GetSubText( i ) );
                }

                textWriter.WriteLine();
                textWriter.WriteLine();
                textWriter.WriteLine( "Legend:" );

                foreach ( var classification in Enum.GetValues( typeof( TextSpanClassification ) ) )
                {
                    textWriter.WriteLine( $"<span class='{classification}'>{classification}</span>" );
                }

                textWriter.WriteLine( "</pre></body>" );
                textWriter.WriteLine( "</html>" );
            }

            result.Success = true;

            return result;
        }
    }
}
