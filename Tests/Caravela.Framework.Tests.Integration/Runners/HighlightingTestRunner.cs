// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Formatting;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable StringLiteralTypo

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal class HighlightingTestRunner : AspectTestRunner
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

            .legend 
            {
                margin-top: 100px;
            }
        </style>
    </head>
    <body>";

        private readonly string _htmlEpilogue;

        public HighlightingTestRunner(
            IServiceProvider serviceProvider,
            string? projectDirectory,
            IEnumerable<MetadataReference> metadataReferences,
            ITestOutputHelper? logger )
            : base( serviceProvider, projectDirectory, metadataReferences, logger )
        {
            StringBuilder epilogueBuilder = new();

            epilogueBuilder.AppendLine( "       <p class='legend'>Legend:</p>" );
            epilogueBuilder.AppendLine( "       <pre>" );

            foreach ( var classification in Enum.GetValues( typeof(TextSpanClassification) ) )
            {
                epilogueBuilder.AppendLine( $"<span class='cr-{classification}'>{classification}</span>" );
            }

            epilogueBuilder.AppendLine( "       </pre>" );
            epilogueBuilder.AppendLine( "   </body>" );
            epilogueBuilder.AppendLine( "</html>" );

            this._htmlEpilogue = epilogueBuilder.ToString();
        }

        protected override HtmlCodeWriter CreateHtmlCodeWriter( TestOptions options )
            => new( new HtmlCodeWriterOptions( options.AddHtmlTitles.GetValueOrDefault(), _htmlProlog, this._htmlEpilogue ) );

        public override void ExecuteAssertions( TestInput testInput, TestResult testResult )
        {
            // We do NOT run the base assertions because we don't want the *.t.cs files, and this is not the point of these tests anyway.

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

                    CompareHtmlFiles( syntaxTree.HtmlInputRunTimePath!, expectedInputHtmlPath );
                }
            }

            // Output.
            if ( testInput.Options.WriteOutputHtml.GetValueOrDefault() )
            {
                var expectedOutputHtmlPath = Path.Combine(
                    Path.GetDirectoryName( testInput.FullPath )!,
                    Path.GetFileNameWithoutExtension( testInput.FullPath ) + FileExtensions.OutputHtml );

                CompareHtmlFiles( testResult.OutputHtmlPath!, expectedOutputHtmlPath );
            }
        }

        private static void CompareHtmlFiles( string actualHtmlPath, string expectedHtmlPath )
        {
            Assert.True( File.Exists( expectedHtmlPath ) );

            var expectedHighlightedSource = NormalizeEndOfLines( File.ReadAllText( expectedHtmlPath ) );

            var htmlPath = actualHtmlPath!;
            var htmlContent = NormalizeEndOfLines( File.ReadAllText( htmlPath ) );

            Assert.Equal( expectedHighlightedSource, htmlContent );
        }
    }
}