// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Formatting
{
    public sealed class HtmlCodeWriter : FormattedCodeWriter
    {
        private readonly HtmlCodeWriterOptions _options;

        public HtmlCodeWriter( IServiceProvider serviceProvider, HtmlCodeWriterOptions options ) : base( serviceProvider )
        {
            this._options = options;
        }

        public async Task WriteAsync( Document document, TextWriter textWriter )
        {
            var sourceText = await document.GetTextAsync( CancellationToken.None );

            var classifiedTextSpans = await this.GetClassifiedTextSpansAsync( document, addTitles: this._options.AddTitles );

            if ( this._options.Prolog != null )
            {
                await textWriter.WriteAsync( this._options.Prolog );
            }

            await textWriter.WriteAsync( "<pre><code class=\"nohighlight\">" );

            foreach ( var classifiedSpan in classifiedTextSpans )
            {
                // Write the text between the previous span and the current one.
                var textSpan = classifiedSpan.Span;

                var subText = sourceText.GetSubText( textSpan );
                var spanText = subText.ToString();

                if ( classifiedSpan.Classification != TextSpanClassification.Excluded )
                {
                    List<string> classes = new();
                    List<string> titles = new();

                    var isLeadingTrivia = string.IsNullOrWhiteSpace( spanText ) && (spanText[0] == '\r' || spanText[0] == '\n');

                    if ( !isLeadingTrivia )
                    {
                        if ( classifiedSpan.Classification != TextSpanClassification.Default )
                        {
                            classes.Add( $"cr-{classifiedSpan.Classification}" );
                        }

                        if ( classifiedSpan.Tags.TryGetValue( CSharpClassTagName, out var csClassification ) )
                        {
                            foreach ( var classification in csClassification.Split( ';' ) )
                            {
                                foreach ( var c in classification.Split( '-' ) )
                                {
                                    classes.Add( "cs-" + c.Trim().Replace( " ", "-" ) );
                                }
                            }
                        }

                        if ( classifiedSpan.Tags.TryGetValue( DiagnosticTagName, out var diagnosticJson ) )
                        {
                            var diagnostic = DiagnosticAnnotation.FromJson( diagnosticJson );
                            titles.Add( diagnostic.ToString() );

                            classes.Add( "diag-" + diagnostic.Severity );
                        }

                        string? docTitle = null;

                        if ( this._options.AddTitles && !classifiedSpan.Tags.TryGetValue( "title", out docTitle ) )
                        {
                            docTitle = classifiedSpan.Classification switch
                            {
                                TextSpanClassification.Dynamic => "Dynamic member.",
                                TextSpanClassification.CompileTime => "Compile-time code.",
                                TextSpanClassification.RunTime => "Run-time code.",
                                TextSpanClassification.TemplateKeyword => "Meta API.",
                                TextSpanClassification.CompileTimeVariable => "Compile-time variable.",
                                TextSpanClassification.GeneratedCode => "Generated code.",
                                _ => null
                            };
                        }

                        if ( docTitle != null )
                        {
                            titles.Insert( 0, docTitle );
                        }
                    }

                    if ( classes.Count > 0 || titles.Count > 0 )
                    {
                        await textWriter.WriteAsync( "<span" );

                        if ( classes.Count > 0 )
                        {
                            await textWriter.WriteAsync( $" class=\"{string.Join( " ", classes )}\"" );
                        }

                        if ( titles.Count > 0 )
                        {
                            var joined = string.Join( "&#13;&#10;", titles.Select( t => HtmlEncode( t, true ) ) );
                            await textWriter.WriteAsync( $" title=\"{joined}\"" );
                        }

                        await textWriter.WriteAsync( ">" );
                        await textWriter.WriteAsync( HtmlEncode( spanText ) );
                        await textWriter.WriteAsync( "</span>" );
                    }
                    else
                    {
                        await textWriter.WriteAsync( HtmlEncode( spanText ) );
                    }
                }
            }

            await textWriter.WriteLineAsync( "</code></pre>" );

            if ( this._options.Epilogue != null )
            {
                await textWriter.WriteAsync( this._options.Epilogue );
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