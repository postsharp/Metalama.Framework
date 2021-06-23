// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable StringLiteralTypo

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal class HighlightingTestRunner : BaseTestRunner
    {
        public HighlightingTestRunner(
            IServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, metadataReferences, logger ) { }

        public override TestResult RunTest( TestInput testInput )
        {
            var result = base.RunTest( testInput );

            if ( !result.Success )
            {
                return result;
            }

            var templateDocument = result.SyntaxTrees.Single().InputDocument;
            var templateSyntaxRoot = templateDocument.GetSyntaxRootAsync().Result!;
            var templateSemanticModel = templateDocument.GetSemanticModelAsync().Result!;

            DiagnosticList diagnostics = new();

            var templateCompiler = new TemplateCompiler( this.ServiceProvider, result.InputCompilation! );

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

            result.SyntaxTrees.Single().AnnotatedSyntaxRoot = annotatedTemplateSyntax;

            this.WriteHtml( testInput, result );

            return result;
        }

        public override void ExecuteAssertions( TestInput testInput, TestResult testResult )
        {
            Assert.NotNull( testInput.ProjectDirectory );
            Assert.NotNull( testInput.RelativePath );

            Assert.True( testResult.Success, testResult.ErrorMessage );

            var sourceAbsolutePath = Path.Combine( testInput.ProjectDirectory!, testInput.RelativePath! );

            var expectedHighlightedPath = Path.Combine(
                Path.GetDirectoryName( sourceAbsolutePath )!,
                Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + FileExtensions.Html );

            Assert.True( File.Exists( expectedHighlightedPath ), $"The file '{expectedHighlightedPath}' does not exist." );
            var expectedHighlightedSource = File.ReadAllText( expectedHighlightedPath );

            var htmlPath = testResult.SyntaxTrees.Single().OutputHtmlPath!;
            var htmlContent = File.ReadAllText( htmlPath );

            Assert.Equal( expectedHighlightedSource, htmlContent );
        }

        protected override void WriteHtmlProlog( TextWriter textWriter )
        {
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

.legend 
{
    margin-top: 100px;
}
" );

            textWriter.WriteLine( "</style>" );
            textWriter.WriteLine( "</head>" );
            textWriter.WriteLine( "<body>" );
        }

        protected override void WriteHtmlEpilogue( TextWriter textWriter )
        {
            textWriter.WriteLine( "<p class='legend'>Legend:</p>" );
            textWriter.WriteLine( "<pre>" );

            foreach ( var classification in Enum.GetValues( typeof(TextSpanClassification) ) )
            {
                textWriter.WriteLine( $"<span class='{classification}'>{classification}</span>" );
            }

            textWriter.WriteLine( "</pre></body>" );
            textWriter.WriteLine( "</html>" );
        }
    }
}