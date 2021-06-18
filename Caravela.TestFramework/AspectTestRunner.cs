// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit.Abstractions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Executes aspect integration tests by running the full aspect pipeline on the input source file.
    /// </summary>
    public partial class AspectTestRunner : BaseTestRunner
    {
        public AspectTestRunner(
            IServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, metadataReferences, logger ) { }

        /// <summary>
        /// Runs the aspect test with the given name and source.
        /// </summary>
        /// <returns>The result of the test execution.</returns>
        public override TestResult RunTest( TestInput testInput )
        {
            var testResult = base.RunTest( testInput );

            using var testProjectOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();

            var pipeline = new CompileTimeAspectPipeline( testProjectOptions, domain, true, testProjectOptions );
            var spy = new Spy( testResult );
            pipeline.ServiceProvider.AddService<ICompileTimeCompilationBuilderSpy>( spy );
            pipeline.ServiceProvider.AddService<ITemplateCompilerSpy>( spy );

            if ( pipeline.TryExecute( testResult, testResult.InputCompilation!, CancellationToken.None, out var resultCompilation, out _ ) )
            {
                testResult.OutputCompilation = resultCompilation;

                if ( testInput.Options.IncludeFinalDiagnostics.GetValueOrDefault() )
                {
                    testResult.Report( resultCompilation.GetDiagnostics().Where( d => d.Severity >= DiagnosticSeverity.Warning ) );
                }
                
                testResult.SetOutputCompilation( resultCompilation );
            }
            else
            {
                testResult.SetFailed( "CompileTimeAspectPipeline.TryExecute failed." );
            }

            if ( testInput.Options.WriteFormattedHtml.GetValueOrDefault() )
            {
                foreach ( var syntaxTree in testResult.SyntaxTrees )
                {
                    this.WriteHtml( testInput, syntaxTree );
                }
            }

            return testResult;
        }

        private void WriteHtml( TestInput testInput, TestSyntaxTree testSyntaxTree )
        {
            var htmlPath = Path.Combine(
                this.ProjectDirectory!,
                "obj",
                "highlighted",
                Path.GetDirectoryName( testInput.RelativePath ) ?? "",
                Path.GetFileNameWithoutExtension( testInput.RelativePath ) + FileExtensions.Html );

            var htmlDirectory = Path.GetDirectoryName( htmlPath );

            if (!Directory.Exists( htmlDirectory ))
            {
                Directory.CreateDirectory( htmlDirectory );
            }

            this.Logger?.WriteLine( "HTML output file: " + htmlPath );

            var sourceText = testSyntaxTree.InputDocument.GetTextAsync().Result; 
            var classifier = new TextSpanClassifier( sourceText, detectRegion: true );
            classifier.Visit( testSyntaxTree.AnnotatedSyntaxRoot );

            using var textWriter = File.CreateText( htmlPath );
            textWriter.Write( "<pre><code class=\"lang-csharp\">" );

            var i = 0;

            foreach (var classifiedSpan in classifier.ClassifiedTextSpans)
            {
                // Write the text between the previous span and the current one.
                if (i < classifiedSpan.Span.Start)
                {
                    var textBefore = sourceText.GetSubText( new TextSpan( i, classifiedSpan.Span.Start - i ) ).ToString();
                    textWriter.Write( HtmlEncode( textBefore ) );
                }

                var spanText = sourceText.GetSubText( classifiedSpan.Span ).ToString();

                if (classifiedSpan.Classification != TextSpanClassification.Excluded)
                {
                    textWriter.Write( $"<span class='caravelaClassification_{classifiedSpan.Classification}'" );

                    if (testInput.Options.AddHtmlTitles.GetValueOrDefault())
                    {
                        var title = classifiedSpan.Classification switch
                        {
                            TextSpanClassification.Dynamic => "Dynamic member",
                            TextSpanClassification.CompileTime => "Compile-time code",
                            TextSpanClassification.RunTime => "Run-time code",
                            TextSpanClassification.TemplateKeyword => "Special member",
                            TextSpanClassification.CompileTimeVariable => "Compile-time variable",
                            _ => null
                        };

                        if (title != null)
                        {
                            textWriter.Write( $" title='{title}'" );
                        }
                    }

                    textWriter.Write( ">" );
                    textWriter.Write( HtmlEncode( spanText ) );
                    textWriter.Write( "</span>" );
                }

                i = classifiedSpan.Span.End;
            }

            // Write the remaining text.
            if (i < sourceText.Length)
            {
                textWriter.Write( HtmlEncode( sourceText.GetSubText( i ).ToString() ) );
            }

            textWriter.WriteLine( "</code></pre>" );
        }

        private static string HtmlEncode( string s )
        {
            var stringBuilder = new StringBuilder( s.Length );

            foreach ( var c in s )
            {
                switch ( c )
                {
                    case '<':
                        stringBuilder.Append( "&lt;" );

                        break;

                    case '>':
                        stringBuilder.Append( "&gt;" );

                        break;

                    case '&':
                        stringBuilder.Append( "&amp;" );

                        break;

                    default:
                        stringBuilder.Append( c );

                        break;
                }
            }

            return stringBuilder.ToString();
        }

        // We don't want the base class to report errors in the input compilation because the pipeline does.
        protected override bool ReportInvalidInputCompilation => false;
    }
}