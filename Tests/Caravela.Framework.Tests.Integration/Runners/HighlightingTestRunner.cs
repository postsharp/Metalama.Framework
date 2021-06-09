﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Net;
using System.Threading;
using Xunit;

// ReSharper disable StringLiteralTypo

namespace Caravela.Framework.Tests.Integration.Runners
{

    internal class HighlightingTestRunner : BaseTestRunner
    {
        public HighlightingTestRunner( IServiceProvider serviceProvider, string? projectDirectory ) : base( serviceProvider, projectDirectory ) { }

        protected override TestResult CreateTestResult() => new HighlightingTestResult();

        public override TestResult RunTest( TestInput testInput )
        {
            var result = base.RunTest( testInput );

            if ( !result.Success )
            {
                return result;
            }

            var templateSyntaxRoot = result.TemplateDocument!.GetSyntaxRootAsync().Result!;
            var templateSemanticModel = result.TemplateDocument.GetSemanticModelAsync().Result!;

            DiagnosticList diagnostics = new();

            var templateCompiler = new TemplateCompiler( this.ServiceProvider, result.InitialCompilation! );

            var templateCompilerSuccess = templateCompiler.TryAnnotate(
                templateSyntaxRoot,
                templateSemanticModel,
                diagnostics,
                CancellationToken.None,
                out var annotatedTemplateSyntax );

            if ( !templateCompilerSuccess )
            {
                result.Report( diagnostics );
                result.SetFailed( "TemplateCompiler.TryAnnotate failed." );

                return result;
            }

            result.AnnotatedTemplateSyntax = annotatedTemplateSyntax;

            if ( this.ProjectDirectory != null )
            {
                var highlightedTemplateDirectory = Path.Combine(
                    this.ProjectDirectory,
                    "obj",
                    "highlighted",
                    Path.GetDirectoryName( testInput.TestName ) ?? "" );

                var highlightedTemplatePath = Path.Combine(
                    highlightedTemplateDirectory,
                    Path.GetFileNameWithoutExtension( testInput.TestName ) + FileExtensions.Html );

                Directory.CreateDirectory( Path.GetDirectoryName( highlightedTemplatePath ) );

                var sourceText = result.TemplateDocument.GetTextAsync().Result;
                var classifier = new TextSpanClassifier( sourceText );
                classifier.Visit( result.AnnotatedTemplateSyntax );

                var textWriter = new StringWriter();
                textWriter.WriteLine( "<html>" );
                textWriter.WriteLine( "<head>" );
                textWriter.WriteLine( "<style>" );

                textWriter.WriteLine(
                    @"
.caravelaClassification_CompileTime,
.caravelaClassification_Conflict,
.caravelaClassification_TemplateKeyword,
.caravelaClassification_Dynamic,
.caravelaClassification_CompileTimeVariable
{
    background-color: rgba(50,50,90,0.1);
}

.caravelaClassification_TemplateKeyword
{
    color: rgb(250, 0, 250) !important;
    font-weight: bold;
}

.caravelaClassification_Dynamic
{
    text-decoration: underline;
}

.caravelaClassification_CompileTimeVariable
{
    font-style: italic;
}
" );

                textWriter.WriteLine( "</style>" );
                textWriter.WriteLine( "</head>" );
                textWriter.WriteLine( "<body><pre>" );

                var i = 0;

                foreach ( var classifiedSpan in classifier.ClassifiedTextSpans )
                {
                    if ( i < classifiedSpan.Span.Start )
                    {
                        textWriter.Write( sourceText.GetSubText( new TextSpan( i, classifiedSpan.Span.Start - i ) ) );
                    }

                    textWriter.Write(
                        $"<span class='caravelaClassification_{classifiedSpan.Classification}'>"
                        + WebUtility.HtmlEncode( sourceText.GetSubText( classifiedSpan.Span ).ToString() )
                        + "</span>" );

                    i = classifiedSpan.Span.End;
                }

                if ( i < sourceText.Length )
                {
                    textWriter.Write( sourceText.GetSubText( i ) );
                }

                textWriter.WriteLine();
                textWriter.WriteLine();
                textWriter.WriteLine( "Legend:" );

                foreach ( var classification in Enum.GetValues( typeof(TextSpanClassification) ) )
                {
                    textWriter.WriteLine( $"<span class='{classification}'>{classification}</span>" );
                }

                textWriter.WriteLine( "</pre></body>" );
                textWriter.WriteLine( "</html>" );

                File.WriteAllText( highlightedTemplatePath, textWriter.ToString() );
                ((HighlightingTestResult) result).OutputHtml = textWriter.ToString();
            }

            return result;
        }

        public override void ExecuteAssertions( TestInput testInput, TestResult testResult )
        {
            Assert.NotNull( testInput.BaseDirectory );
            Assert.NotNull( testInput.RelativePath );

            Assert.True( testResult.Success, testResult.ErrorMessage );

            var sourceAbsolutePath = Path.Combine( testInput.BaseDirectory!, testInput.RelativePath! );

            var expectedHighlightedPath = Path.Combine(
                Path.GetDirectoryName( sourceAbsolutePath )!,
                Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + FileExtensions.Html );

            var expectedHighlightedSource = File.ReadAllText( expectedHighlightedPath );

            Assert.Equal( expectedHighlightedSource, ((HighlightingTestResult) testResult).OutputHtml );
        }
    }
}