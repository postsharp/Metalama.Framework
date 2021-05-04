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
using System.Text;
using System.Threading.Tasks;

// ReSharper disable StringLiteralTypo

namespace Caravela.Framework.Tests.Integration.Highlighting
{
    internal class HighlightingTestRunner : TestRunnerBase
    {
        public HighlightingTestRunner( string projectDirectory ) : base( projectDirectory ) { }

        public override async Task<TestResult> RunTestAsync( TestInput testInput )
        {
            var result = await base.RunTestAsync( testInput );

            if ( !result.Success )
            {
                return result;
            }

            var templateSyntaxRoot = (await result.TemplateDocument.GetSyntaxRootAsync())!;
            var templateSemanticModel = (await result.TemplateDocument.GetSemanticModelAsync())!;

            DiagnosticList diagnostics = new();

            var templateCompilerSuccess = TemplateCompiler.TryAnnotate(
                templateSyntaxRoot,
                templateSemanticModel,
                diagnostics,
                out var annotatedTemplateSyntax );

            if ( !templateCompilerSuccess )
            {
                result.ReportDiagnostics( diagnostics );
                result.SetFailed( "TemplateCompiler.TryAnnotate compiler failed." );

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
                    Path.GetFileNameWithoutExtension( testInput.TestName ) + ".highlighted.html" );

                Directory.CreateDirectory( Path.GetDirectoryName( highlightedTemplatePath ) );

                var sourceText = await result.TemplateDocument.GetTextAsync();
                var classifier = new TextSpanClassifier( sourceText );
                classifier.Visit( result.AnnotatedTemplateSyntax );

                await using ( var textWriter = new StreamWriter( highlightedTemplatePath, false, Encoding.UTF8 ) )
                {
                    textWriter.WriteLine( "<html>" );
                    textWriter.WriteLine( "<head>" );
                    textWriter.WriteLine( "<style>" );

                    textWriter.WriteLine(
                        @"
.CompileTime {
    background-color: #E8F2FF;
}
.CompileTimeVariable {
    background-color: #C6D1DD;
}
.RunTime {
    background-color: antiquewhite;
}
.TemplateKeyword {
    background-color: #FFFF22;
}
.Dynamic {
    background-color: #FFFFBB;
}
.Conflict {
    background-color: red;
}
.Default {
    background-color: lightcoral;
}" );

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
                            $"<span class='{classifiedSpan.Classification}'>" + WebUtility.HtmlEncode( sourceText.GetSubText( classifiedSpan.Span ).ToString() )
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
                }
            }

            return result;
        }
    }
}