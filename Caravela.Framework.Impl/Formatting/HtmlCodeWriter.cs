// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Caravela.Framework.Impl.Formatting
{
    public sealed partial class HtmlCodeWriter
    {
        private readonly HtmlCodeWriterOptions _options;

        public HtmlCodeWriter( HtmlCodeWriterOptions options )
        {
            this._options = options;
        }

        public void Write( Document document, SyntaxNode? annotatedSyntaxRoot, TextWriter textWriter )
        {
            var sourceText = document.GetTextAsync().Result;

            ClassifiedTextSpanCollection classifiedTextSpans;
            
            if ( annotatedSyntaxRoot != null )
            {
                var classifier = new TextSpanClassifier( sourceText, detectRegion: true );
                classifier.Visit( annotatedSyntaxRoot );
                classifiedTextSpans = (ClassifiedTextSpanCollection) classifier.ClassifiedTextSpans;
            }
            else
            {
                classifiedTextSpans = new();
            }

            if ( this._options.Prolog != null )
            {
                textWriter.Write( this._options.Prolog );
            }

            textWriter.Write( "<pre><code class=\"nohighlight\">" );

            var i = 0;

            // Add C# classifications
            var syntaxTree = document.GetSyntaxTreeAsync().Result!;
            var semanticModel = document.Project.GetCompilationAsync().Result!.GetSemanticModel( syntaxTree );

            foreach ( var csharpSpan in Classifier.GetClassifiedSpans(
                semanticModel,
                syntaxTree.GetRoot().Span,
                document.Project!.Solution.Workspace ) )
            {
                classifiedTextSpans.SetTag( csharpSpan.TextSpan, "class", csharpSpan.ClassificationType );
            }

            // Add XML doc based on the input compilation.
            if ( this._options.AddTitles )
            {
                var visitor = new AddTitlesVisitor( classifiedTextSpans, semanticModel );
                visitor.Visit( syntaxTree.GetRoot() );
            }

            foreach ( var classifiedSpan in classifiedTextSpans )
            {
                // Write the text between the previous span and the current one.
                if ( i < classifiedSpan.Span.Start )
                {
                    var textBefore = sourceText.GetSubText( new TextSpan( i, classifiedSpan.Span.Start - i ) ).ToString();
                    textWriter.Write( HtmlEncode( textBefore ) );
                }

                var spanText = sourceText.GetSubText( classifiedSpan.Span ).ToString();

                if ( classifiedSpan.Classification != TextSpanClassification.Excluded )
                {
                    List<string> classes = new();
                    string? title = null;

                    if ( classifiedSpan.Classification != TextSpanClassification.Default )
                    {
                        classes.Add( $"cr-{classifiedSpan.Classification}" );
                    }

                    if ( classifiedSpan.Tags.TryGetValue( "class", out var csClassification ) )
                    {
                        foreach ( string c in csClassification.Split( '-' ) )
                        {
                            classes.Add( "cs-" + c.Trim().Replace( " ", "-" ) );
                        }
                    }

                    if ( this._options.AddTitles && !classifiedSpan.Tags.TryGetValue( "title", out title ) )
                    {
                        title = classifiedSpan.Classification switch
                        {
                            TextSpanClassification.Dynamic => "Dynamic member.",
                            TextSpanClassification.CompileTime => "Compile-time code.",
                            TextSpanClassification.RunTime => "Run-time code.",
                            TextSpanClassification.TemplateKeyword => "Meta API.",
                            TextSpanClassification.CompileTimeVariable => "Compile-time variable.",
                            _ => null
                        };
                    }

                    if ( classes.Count > 0 || !string.IsNullOrEmpty( title ) )
                    {
                        textWriter.Write( "<span" );

                        if ( classes.Count > 0 )
                        {
                            textWriter.Write( $" class=\"{string.Join( " ", classes )}\"" );
                        }

                        if ( !string.IsNullOrEmpty( title ) )
                        {
                            textWriter.Write( $" title=\"{HtmlEncode( title!, true )}\"" );
                        }
                        
                        textWriter.Write( ">" );
                        textWriter.Write( HtmlEncode( spanText ) );
                        textWriter.Write( "</span>" );
                    }
                    else
                    {
                        textWriter.Write( HtmlEncode( spanText ) );
                    }
                }

                i = classifiedSpan.Span.End;
            }

            // Write the remaining text.
            if ( i < sourceText.Length )
            {
                textWriter.Write( HtmlEncode( sourceText.GetSubText( i ).ToString() ) );
            }

            textWriter.WriteLine( "</code></pre>" );

            if ( this._options.Epilogue != null )
            {
                textWriter.Write( this._options.Epilogue );
            }
        }

        private static string HtmlEncode( string s, bool attributeEncode = false )
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

                    case '"' when attributeEncode:
                        stringBuilder.Append( "&quot;" );

                        break;

                    case '\r' when attributeEncode:
                        break;

                    case '\n' when attributeEncode:
                        stringBuilder.Append( "&#10;" );

                        break;

                    default:
                        stringBuilder.Append( c );

                        break;
                }
            }

            return stringBuilder.ToString();
        }
    }
}