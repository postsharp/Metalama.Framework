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
using System.Net;
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

            if ( pipeline.TryExecute( testResult, testResult.InitialCompilation!, CancellationToken.None, out var resultCompilation, out _ ) )
            {
                testResult.ResultCompilation = resultCompilation;
                var syntaxRoot = resultCompilation.SyntaxTrees.First().GetRoot();

                if ( testInput.Options.IncludeFinalDiagnostics.GetValueOrDefault() )
                {
                    testResult.Report( resultCompilation.GetDiagnostics().Where( d => d.Severity >= DiagnosticSeverity.Warning ) );
                }

                testResult.SetTransformedTarget( syntaxRoot );
            }
            else
            {
                testResult.SetFailed( "CompileTimeAspectPipeline.TryExecute failed." );
            }

            if ( testInput.Options.WriteFormattedHtml )
            {
                var htmlPath = Path.Combine(
                    this.ProjectDirectory!,
                    "obj",
                    "highlighted",
                    Path.GetDirectoryName( testInput.RelativePath ) ?? "",
                    Path.GetFileNameWithoutExtension( testInput.RelativePath ) + FileExtensions.Html );

                var htmlDirectory = Path.GetDirectoryName( htmlPath );

                if ( !Directory.Exists( htmlDirectory ) )
                {
                    Directory.CreateDirectory( htmlDirectory );
                }

                this.Logger?.WriteLine( "HTML output file: " + htmlPath );

                var sourceText = testResult.TemplateDocument!.GetTextAsync().Result;
                var classifier = new TextSpanClassifier( sourceText, detectRegion: true );
                classifier.Visit( testResult.AnnotatedTemplateSyntax );

                using var textWriter = File.CreateText( htmlPath );
                textWriter.Write( "<pre><code class=\"lang-csharp\">" );

                var i = 0;

                foreach ( var classifiedSpan in classifier.ClassifiedTextSpans )
                {
                    // Write the text between the previous span and the current one.
                    if ( i < classifiedSpan.Span.Start )
                    {
                        var textBefore = sourceText.GetSubText( new TextSpan( i, classifiedSpan.Span.Start - i ) ).ToString();
                        textWriter.Write( textBefore );
                    }

                    var spanText = sourceText.GetSubText( classifiedSpan.Span ).ToString();

                    if ( classifiedSpan.Classification != TextSpanClassification.Excluded )
                    {
                        textWriter.Write( $"<span class='caravelaClassification_{classifiedSpan.Classification}'" );

                        if ( testInput.Options.AddHtmlTitles )
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

                            if ( title != null )
                            {
                                textWriter.Write( $" title='{title}'" );
                            }
                        }

                        textWriter.Write( ">" );
                        textWriter.Write( WebUtility.HtmlEncode( spanText ) );
                        textWriter.Write( "</span>" );
                    }

                    i = classifiedSpan.Span.End;
                }

                // Write the remaining text.
                if ( i < sourceText.Length )
                {
                    textWriter.Write( sourceText.GetSubText( i ) );
                }

                textWriter.WriteLine( "</code></pre>" );
            }

            return testResult;
        }

        // We don't want the base class to report errors in the input compilation because the pipeline does.
        protected override bool ReportInvalidInputCompilation => false;
    }
}