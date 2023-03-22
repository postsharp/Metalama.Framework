// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Formatting
{
    public sealed class HtmlCodeWriter : FormattedCodeWriter
    {
        private readonly HtmlCodeWriterOptions _options;

        public HtmlCodeWriter( ProjectServiceProvider serviceProvider, HtmlCodeWriterOptions options ) : base( serviceProvider )
        {
            this._options = options;
        }

        public async Task WriteAsync( Document document, TextWriter textWriter, IEnumerable<Diagnostic>? diagnostics = null )
        {
            var sourceText = await document.GetTextAsync( CancellationToken.None );

            var classifiedTextSpans = await this.GetClassifiedTextSpansAsync( document, addTitles: this._options.AddTitles, diagnostics: diagnostics );

            var finalBuilder = new StringBuilder();      // Builds the whole file.
            var codeLineBuilder = new StringBuilder();   // Builds the current line.
            var diagnosticBuilder = new StringBuilder(); // Builds the diagnostics of the current line.
            var diagnosticSet = new HashSet<string>( StringComparer.Ordinal );

            void FlushLine()
            {
                if ( diagnosticBuilder.Length > 0 )
                {
                    // Figure out the indentation of the next block.
                    var indentation = "";
                    var isTag = false;

                    foreach ( var c in codeLineBuilder.ToString() )
                    {
                        switch ( c )
                        {
                            case '<':
                                isTag = true;

                                break;

                            case '>':
                                isTag = false;

                                break;

                            case ' ' or '\t' when !isTag:
                                indentation += c;

                                break;

                            default:
                                if ( !isTag )
                                {
                                    goto parsingIndentationFinished;
                                }
                                else
                                {
                                    break;
                                }
                        }
                    }

                parsingIndentationFinished:

                    // Write any buffered diagnostic.
                    var diagnosticLines = diagnosticBuilder.ToString().Split( '\n' );

                    finalBuilder.Append( "<span class=\"diagLines\">" );

                    foreach ( var diagnostic in diagnosticLines )
                    {
                        finalBuilder.Append( indentation );
                        finalBuilder.Append( diagnostic );
                        finalBuilder.Append( '\n' );
                    }

                    finalBuilder.Append( "</span>" );

                    diagnosticBuilder.Clear();
                    diagnosticSet.Clear();
                }

                // Then write the buffered code.
                finalBuilder.Append( codeLineBuilder );
                codeLineBuilder.Clear();
            }

            if ( this._options.Prolog != null )
            {
                finalBuilder.Append( this._options.Prolog );
            }

            finalBuilder.Append( "<pre><code class=\"nohighlight\">" );

            var isTopOfTheFile = true;

            foreach ( var classifiedSpan in classifiedTextSpans )
            {
                // Write the text between the previous span and the current one.
                var textSpan = classifiedSpan.Span;

                var subText = sourceText.GetSubText( textSpan );
                var spanText = subText.ToString();

                // Ignore blank lines on the top of the file.
                if ( spanText.Trim().Length == 0 && isTopOfTheFile )
                {
                    continue;
                }

                if ( classifiedSpan.Classification != TextSpanClassification.Excluded )
                {
                    List<string> classes = new();
                    List<string> titles = new();

                    const bool isLeadingTrivia = false; // string.IsNullOrWhiteSpace( spanText ) && (spanText[0] == '\r' || spanText[0] == '\n');

                    if ( !isLeadingTrivia )
                    {
                        if ( classifiedSpan.Classification != TextSpanClassification.Default )
                        {
                            classes.Add( $"cr-{classifiedSpan.Classification}" );
                        }

                        if ( classifiedSpan.Tags.TryGetValue( CSharpClassTagName, out var csClassification ) )
                        {
                            // Ignore the header.
                            if ( csClassification == "header" )
                            {
                                continue;
                            }

                            foreach ( var classification in csClassification.Split( ';' ) )
                            {
                                foreach ( var c in classification.Split( '-' ) )
                                {
                                    classes.Add( "cs-" + c.Trim().ReplaceOrdinal( " ", "-" ) );
                                }
                            }
                        }

                        isTopOfTheFile = false;

                        if ( classifiedSpan.Tags.TryGetValue( DiagnosticTagName, out var diagnosticJson ) )
                        {
                            var diagnostic = DiagnosticAnnotation.FromJson( diagnosticJson );

                            if ( diagnostic.Severity != DiagnosticSeverity.Hidden && diagnosticSet.Add( diagnostic.Message ) )
                            {
                                titles.Add( diagnostic.ToString() );

                                classes.Add( "diag-" + diagnostic.Severity );

                                diagnosticBuilder.AppendInvariant( $"<span class=\"diagLine-{diagnostic.Severity}\">{diagnostic.Severity} {diagnostic.Id}: " );

                                HtmlEncode( diagnosticBuilder, diagnostic.Message );
                                diagnosticBuilder.Append( "</span>\n" );
                            }
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
                                TextSpanClassification.GeneratedCode when classifiedSpan.Tags.TryGetValue( GeneratingAspectTagName, out var aspect ) =>
                                    $"Generated by {aspect}.",
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
                        codeLineBuilder.Append( "<span" );

                        if ( classes.Count > 0 )
                        {
                            codeLineBuilder.AppendInvariant( $" class=\"{string.Join( " ", classes )}\"" );
                        }

                        if ( titles.Count > 0 )
                        {
                            codeLineBuilder.Append( " title=\"" );

                            for ( var i = 0; i < titles.Count; i++ )
                            {
                                if ( i > 0 )
                                {
                                    codeLineBuilder.Append( "&#13;&#10;" );
                                }

                                HtmlEncode( codeLineBuilder, titles[i], true );
                            }

                            codeLineBuilder.Append( "\"" );
                        }

                        codeLineBuilder.Append( ">" );
                        HtmlEncode( codeLineBuilder, spanText, onNewLine: FlushLine );
                        codeLineBuilder.Append( "</span>" );
                    }
                    else
                    {
                        HtmlEncode( codeLineBuilder, spanText, onNewLine: FlushLine );
                    }
                }
            }

            FlushLine();

            finalBuilder.AppendLine( "</code></pre>" );

            if ( this._options.Epilogue != null )
            {
                finalBuilder.Append( this._options.Epilogue );
            }

            await textWriter.WriteAsync( finalBuilder.ToString() );
        }

        private static void HtmlEncode( StringBuilder stringBuilder, string s, bool attributeEncode = false, Action? onNewLine = null )
        {
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

                    case '\n':
                        stringBuilder.Append( c );

                        onNewLine?.Invoke();

                        break;

                    default:
                        stringBuilder.Append( c );

                        break;
                }
            }
        }

        internal static async Task WriteAllAsync(
            IProjectOptions projectOptions,
            ProjectServiceProvider serviceProvider,
            PartialCompilation partialCompilation,
            string suffix = "" )
        {
            var compilation = partialCompilation.Compilation;
            var writer = new HtmlCodeWriter( serviceProvider, new HtmlCodeWriterOptions( true ) );

            var workspace = new AdhocWorkspace();

            var assemblyName = compilation.AssemblyName.AssertNotNull();

            var projectId = ProjectId.CreateNewId( assemblyName );

            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                assemblyName,
                assemblyName,
                "C#",
                projectOptions.ProjectPath,
                compilationOptions: compilation.Options,
                metadataReferences: compilation.References );

            var project = workspace.AddProject( projectInfo );

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                project = project.AddDocument( syntaxTree.FilePath, await syntaxTree.GetRootAsync(), null, syntaxTree.FilePath ).Project;
            }

            var projectDirectory = Path.GetFullPath( Path.GetDirectoryName( projectOptions.ProjectPath )! );
            var outputDirectory = Path.Combine( projectDirectory, "obj", "html", projectOptions.TargetFramework ?? "" );

            foreach ( var document in project.Documents )
            {
                var documentPath = Path.GetFullPath( document.FilePath.AssertNotNull() );

                if ( !documentPath.StartsWith( projectDirectory, StringComparison.OrdinalIgnoreCase ) )
                {
                    // Skipping this document.
                    continue;
                }

                var relativePath = documentPath.Substring( projectDirectory.Length + 1 );
                var outputPath = Path.Combine( outputDirectory, relativePath + suffix + ".html" );

                var outputSubdirectory = Path.GetDirectoryName( outputPath ).AssertNotNull();
                Directory.CreateDirectory( outputSubdirectory );

#if NET6_0_OR_GREATER
                await using var textWriter = new StreamWriter( outputPath );
#else
                using var textWriter = new StreamWriter( outputPath );
#endif
                await writer.WriteAsync( document, textWriter );
            }
        }
    }
}