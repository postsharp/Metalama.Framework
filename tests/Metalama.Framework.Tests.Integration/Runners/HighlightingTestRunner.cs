﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Services;
using Metalama.Testing.AspectTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable StringLiteralTypo

namespace Metalama.Framework.Tests.Integration.Runners
{
    internal sealed class HighlightingTestRunner : AspectTestRunner
    {
        private const string _htmlProlog = @"
<html>
    <head>
        <style>
            .cr-CompileTime,
            .cr-Conflict,
            .cr-TemplateKeyword,
            .cr-Dynamic,
            .cr-CompileTimeVariable,
            .cr-GeneratedCode
            {
                background-color: rgba(50,50,90,0.1);
            }

            .cr-NeutralTrivia
            {
                background-color: rgba(0,255,0,0.1);
            }

            .cr-TemplateKeyword
            {
                color: rgb(250, 0, 250) !important;
                font-weight: bold;
            }

            .cr-Dynamic
            {
                text-decoration: underline;
            }

            .cr-CompileTimeVariable
            {
                font-style: italic;
            }

            .diag-Warning
            {
                text-decoration: underline 1px wavy orange;
            }

            .diag-Error
            {
                text-decoration: underline 1px wavy red;
            }

            .legend 
            {
                margin-top: 100px;
            }
        </style>
    </head>
    <body>";

        private readonly string _htmlEpilogue;

        public HighlightingTestRunner(
            GlobalServiceProvider serviceProvider,
            string? projectDirectory,
            TestProjectReferences references,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, references, logger )
        {
            StringBuilder epilogueBuilder = new();

            epilogueBuilder.AppendLine( "       <p class='legend'>Legend:</p>" );
            epilogueBuilder.AppendLine( "       <pre>" );

            foreach ( var classification in Enum.GetValues( typeof(TextSpanClassification) ) )
            {
                epilogueBuilder.AppendLine( FormattableString.Invariant( $"<span class='cr-{classification}'>{classification}</span>" ) );
            }

            epilogueBuilder.AppendLine( "       </pre>" );
            epilogueBuilder.AppendLine( "   </body>" );
            epilogueBuilder.AppendLine( "</html>" );

            this._htmlEpilogue = epilogueBuilder.ToString();
        }

        protected override bool CompareTransformedCode => false;

        protected override HtmlCodeWriterOptions GetHtmlCodeWriterOptions( TestOptions options )
            => new( options.AddHtmlTitles.GetValueOrDefault(), _htmlProlog, this._htmlEpilogue );

        protected override void ExecuteAssertions( TestInput testInput, TestResult testResult, Dictionary<string, object?> state )
        {
            base.ExecuteAssertions( testInput, testResult, state );

            Assert.NotNull( testInput.ProjectDirectory );
            Assert.NotNull( testInput.RelativePath );
            Assert.True( testResult.Success, testResult.ErrorMessage );

            // Input
            if ( testInput.Options.WriteInputHtml.GetValueOrDefault() )
            {
                foreach ( var syntaxTree in testResult.SyntaxTrees )
                {
                    var sourceAbsolutePath = syntaxTree.InputPath;

                    // Input.
                    var expectedInputHtmlPath = Path.Combine(
                        Path.GetDirectoryName( sourceAbsolutePath )!,
                        Path.GetFileNameWithoutExtension( sourceAbsolutePath ) + FileExtensions.InputHtml );

                    this.CompareHtmlFiles( syntaxTree.HtmlInputRunTimePath!, expectedInputHtmlPath );
                }
            }

            // Output.
            if ( testInput.Options.WriteOutputHtml.GetValueOrDefault() )
            {
                var expectedOutputHtmlPath = Path.Combine(
                    Path.GetDirectoryName( testInput.FullPath )!,
                    Path.GetFileNameWithoutExtension( testInput.FullPath ) + FileExtensions.OutputHtml );

                this.CompareHtmlFiles( testResult.OutputHtmlPath!, expectedOutputHtmlPath );
            }
        }

        private void CompareHtmlFiles( string actualHtmlPath, string expectedHtmlPath )
        {
            this.Logger?.WriteLine( "Actual HTML: " + actualHtmlPath );

            Assert.True( File.Exists( expectedHtmlPath ), $"The expected HTML file '{expectedHtmlPath}' does not exist." );

            this.Logger?.WriteLine( "Expected HTML: " + expectedHtmlPath );

            var expectedHighlightedSource = TestOutputNormalizer.NormalizeEndOfLines( File.ReadAllText( expectedHtmlPath ) );

            var htmlPath = actualHtmlPath;
            var htmlContent = TestOutputNormalizer.NormalizeEndOfLines( File.ReadAllText( htmlPath ) );

            Assert.Equal( expectedHighlightedSource, htmlContent );
        }
    }
}