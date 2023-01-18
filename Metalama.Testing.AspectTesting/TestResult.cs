﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Utilities;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Metalama.Testing.AspectTesting;

/// <summary>
/// Represents the result of a test run.
/// </summary>
[PublicAPI]
internal sealed class TestResult : IDisposable
{
    private static readonly Regex _cleanCallStackRegex = new( " in (.*):line \\d+" );

    private readonly List<TestSyntaxTree> _syntaxTrees = new();
    private bool _frozen;

    public TestInput? TestInput { get; set; }

    public IDiagnosticBag InputCompilationDiagnostics { get; } = new DiagnosticBag();

    public IDiagnosticBag OutputCompilationDiagnostics { get; } = new DiagnosticBag();

    public IDiagnosticBag CompileTimeCompilationDiagnostics { get; } = new DiagnosticBag();

    public IDiagnosticBag PipelineDiagnostics { get; } = new DiagnosticBag();

    public IEnumerable<Diagnostic> Diagnostics
        => this.OutputCompilationDiagnostics
            .Concat( this.PipelineDiagnostics )
            .Concat( this.InputCompilationDiagnostics );

    // We don't add the CompileTimeCompilationDiagnostics to Diagnostics because they are already in PipelineDiagnostics.

    public IReadOnlyList<TestSyntaxTree> SyntaxTrees => this._syntaxTrees;

    /// <summary>
    /// Gets the primary error message emitted during test run.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets or sets the input test project (before transformation).
    /// </summary>
    public Project? InputProject { get; set; }

    /// <summary>
    /// Gets or sets the input test project (after transformation).
    /// </summary>
    public Project? OutputProject { get; set; }

    /// <summary>
    /// Gets or sets the initial compilation of the test project.
    /// </summary>
    public Compilation? InputCompilation { get; set; }

    /// <summary>
    /// Gets or sets the result compilation of the test project.
    /// </summary>
    public Compilation? OutputCompilation { get; set; }

    /// <summary>
    /// Gets or sets the full path of the HTML file with syntax highlighting.
    /// </summary>
    public string? OutputHtmlPath { get; set; }

    /// <summary>
    /// Gets a value indicating whether the test run succeeded.
    /// </summary>
    public bool Success { get; private set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the output run-time code should be included in the result of
    ///  <see cref="GetTestOutputsWithDiagnostics"/>.
    /// </summary>
    public bool HasOutputCode { get; set; }

    public Compilation? CompileTimeCompilation { get; private set; }

    public ICompilation? InitialCompilationModel { get; set; }

    internal PartialCompilation? IntermediateLinkerCompilation { get; set; }

    public string? ProgramOutput { get; set; }

    public TestContext? TestContext { get; set; }

    internal async Task AddInputDocumentAsync( Document document, string? path )
        => this._syntaxTrees.Add( await TestSyntaxTree.CreateAsync( path, document, this ) );

    private static string CleanMessage( string text )
    {
        // Remove local-specific stuff.
        text = _cleanCallStackRegex.Replace( text, "" );

        // Comment all lines but the first one.
        var lines = text.Split( '\n' );

        if ( lines.Length == 1 )
        {
            return text;
        }

        lines[0] = lines[0].Trim( '\r' );

        for ( var i = 1; i < lines.Length; i++ )
        {
            if ( lines[i].TrimStart().StartsWith( "at ", StringComparison.Ordinal ) )
            {
                // Remove exception stacks because they are different in debug and release builds.
                lines[i] = "<>";
            }
            else
            {
                lines[i] = "// " + lines[i].Trim( '\r' );
            }
        }

        return string.Join( Environment.NewLine, lines.Where( l => l != "<>" ) );
    }

    internal async Task SetCompileTimeCompilationAsync( Compilation compilation )
    {
        if ( this.InputCompilation == null ||
             this.TestInput == null ||
             this.InputProject == null )
        {
            throw new InvalidOperationException( "The object has not bee properly initialized." );
        }

        this.CompileTimeCompilation = compilation;

        foreach ( var syntaxTree in compilation.SyntaxTrees )
        {
            if ( CompileTimeConstants.IsPredefinedSyntaxTree( syntaxTree.FilePath ) )
            {
                // This is the "Intrinsics" syntax tree.
                continue;
            }

            var syntaxNode = await syntaxTree.GetRootAsync();

            // Format the output code.
            var annotation = syntaxNode.GetAnnotations( CompileTimeSyntaxAnnotations.OriginalSyntaxTreePath ).SingleOrDefault();

            if ( annotation != null )
            {
                var testTree = this.SyntaxTrees.SingleOrDefault( t => Path.GetFileName( t.InputPath ) == Path.GetFileName( annotation.Data ) );

                testTree?.SetCompileTimeCode( syntaxNode, syntaxTree.FilePath );
            }
        }
    }

    internal async Task SetOutputCompilationAsync( Compilation runTimeCompilation )
    {
        if ( this.InputCompilation == null ||
             this.TestInput == null ||
             this.InputProject == null )
        {
            throw new InvalidOperationException( "The object has not bee properly initialized." );
        }

        foreach ( var syntaxTree in runTimeCompilation.SyntaxTrees )
        {
            var testSyntaxTree = this.SyntaxTrees.SingleOrDefault( x => StringComparer.Ordinal.Equals( x.InputDocument.FilePath, syntaxTree.FilePath ) );

            if ( testSyntaxTree == null )
            {
                // This is the "Intrinsics" syntax tree.
                continue;
            }

            var syntaxNode = await syntaxTree.GetRootAsync();

            // Format the output code.
            await testSyntaxTree.SetRunTimeCodeAsync( syntaxNode );
        }
    }

    public void SetFailed( string reason, Exception? exception = null )
    {
        if ( this._frozen )
        {
            throw new InvalidOperationException( "The test result can no longer be modified." );
        }

        this._frozen = true;

        this.Success = false;
        this.ErrorMessage = reason;

        if ( exception != null )
        {
            this.ErrorMessage += Environment.NewLine + exception;
        }
    }

    private string GetTextUnderDiagnostic( Diagnostic diagnostic )
    {
        var syntaxTree = diagnostic.Location.SourceTree;

        // If we don't have the a SyntaxTree, find it from the input compilation.

        syntaxTree ??= this.InputCompilation?.SyntaxTrees.SingleOrDefault( t => t.FilePath == diagnostic.Location.GetLineSpan().Path );

        var text = syntaxTree?.GetText()
            .GetSubText( diagnostic.Location.SourceSpan )
            .ToString()
            .ReplaceOrdinal( "\r\n", " " )
            .ReplaceOrdinal( "\n", " " )
            .ReplaceOrdinal( "\n", " " )
            .ReplaceOrdinal( "\t", " " ) ?? "";

        while ( text.ContainsOrdinal( "  " ) )
        {
            text = text.ReplaceOrdinal( "  ", " " );
        }

        return text;
    }

    /// <summary>
    /// Gets the content of the <c>.t.cs</c> file, i.e. the output transformed code with comments
    /// for diagnostics.
    /// </summary>
    public IReadOnlyList<SyntaxTree> GetTestOutputsWithDiagnostics()
    {
        if ( this.TestInput == null )
        {
            throw new InvalidOperationException();
        }

        List<SyntaxTree> result = new();

        // Adding the syntax of the transformed run-time code, but only if the pipeline was successful.
        var outputSyntaxTrees =
            this.TestInput.Options.OutputAllSyntaxTrees == true
                ? this.SyntaxTrees.AsEnumerable()
                : this.SyntaxTrees.Take( 1 );

        foreach ( var outputSyntaxTree in outputSyntaxTrees )
        {
            var consolidatedCompilationUnit = SyntaxFactory.CompilationUnit();

            if ( this.HasOutputCode && outputSyntaxTree is { OutputRunTimeSyntaxRoot: not null } && this.TestInput.Options.RemoveOutputCode != true )
            {
                // Adding syntax annotations for the output compilation. We cannot add syntax annotations for diagnostics
                // on the input compilation because they would potentially not map properly to the output compilation.
                var outputSyntaxRoot = FormattedCodeWriter.AddDiagnosticAnnotations(
                    outputSyntaxTree.OutputRunTimeSyntaxRoot,
                    outputSyntaxTree.InputPath,
                    this.OutputCompilationDiagnostics.ToArray() );

                // Add assembly-level custom attributes. We do not include AspectOrder because this would pollute many tests.
                consolidatedCompilationUnit = consolidatedCompilationUnit.WithAttributeLists(
                    consolidatedCompilationUnit.AttributeLists.AddRange(
                        outputSyntaxRoot.AttributeLists.Where( a => !a.ToString().ContainsOrdinal( "AspectOrder" ) ) ) );

                // Find notes annotated with // <target> or with a comment containing <target> and choose the first one. If there is none, the test output is the whole tree
                // passed to this method.

                var outputMembers =
                    outputSyntaxRoot
                        .DescendantNodesAndSelf( _ => true )
                        .OfType<MemberDeclarationSyntax>()
                        .Where(
                            m => m.GetLeadingTrivia().ToString().ContainsOrdinal( "<target>" ) ||
                                 m.ChildTokens().FirstOrDefault().LeadingTrivia.ToString().ContainsOrdinal( "<target>" ) ||
                                 m.AttributeLists.Any( a => a.GetLeadingTrivia().ToString().ContainsOrdinal( "<target>" ) ) )
                        .Cast<SyntaxNode>()
                        .ToArray();

                outputMembers = outputMembers switch
                {
                    { Length: 0 } => new SyntaxNode[] { outputSyntaxRoot },
                    _ => outputMembers
                };

                for ( var i = 0; i < outputMembers.Length; i++ )
                {
                    switch ( outputMembers[i] )
                    {
                        case MemberDeclarationSyntax member:
                            if ( i != outputMembers.Length - 1 )
                            {
                                consolidatedCompilationUnit =
                                    consolidatedCompilationUnit.AddMembers(
                                        member.WithoutTrivia().WithTrailingTrivia( SyntaxFactory.ElasticLineFeed, SyntaxFactory.ElasticLineFeed ) );
                            }
                            else
                            {
                                consolidatedCompilationUnit = consolidatedCompilationUnit.AddMembers( member.WithoutTrivia() );
                            }

                            break;

                        case CompilationUnitSyntax compilationUnit:
                            consolidatedCompilationUnit = consolidatedCompilationUnit
                                .AddMembers( compilationUnit.Members.ToArray() )
                                .AddUsings( compilationUnit.Usings.ToArray() );

                            break;

                        case ExpressionStatementSyntax statementSyntax when statementSyntax.ToString() == "":
                            // This is an empty statement
                            consolidatedCompilationUnit = consolidatedCompilationUnit
                                .WithLeadingTrivia( statementSyntax.GetLeadingTrivia().AddRange( consolidatedCompilationUnit.GetLeadingTrivia() ) );

                            break;

                        default:
                            throw new InvalidOperationException( $"Don't know how to add a {outputMembers[i].Kind()} to the compilation unit." );
                    }
                }
            }

            // Adding the diagnostics as trivia.
            List<SyntaxTrivia> comments = new();

            if ( !this.Success && (this.TestInput!.Options.ReportErrorMessage.GetValueOrDefault()
                                   || this.Diagnostics.All( c => c.Severity != DiagnosticSeverity.Error )) )
            {
                comments.Add( SyntaxFactory.Comment( $"// {this.ErrorMessage} \n" ) );
            }

            // We exclude LAMA0222 from the results because it contains randomly-generated info and tests need to be deterministic.

            comments.AddRange(
                this.Diagnostics
                    .Where(
                        d => d.Id != "LAMA0222" &&
                             (this.TestInput!.Options.IncludeAllSeverities.GetValueOrDefault()
                              || d.Severity >= DiagnosticSeverity.Warning) && !this.TestInput.Options.IgnoredDiagnostics.Contains( d.Id ) )
                    .OrderBy( d => d.Location.SourceSpan.Start )
                    .ThenBy( d => d.GetMessage( CultureInfo.InvariantCulture ), StringComparer.Ordinal )
                    .SelectMany( this.GetDiagnosticComments )
                    .Select( SyntaxFactory.Comment )
                    .ToList() );

            consolidatedCompilationUnit = consolidatedCompilationUnit.WithLeadingTrivia( comments );

            // Individual trees should be formatted, so we don't need to format again.

            result.Add(
                CSharpSyntaxTree.Create(
                    consolidatedCompilationUnit,
                    path: Path.GetFileName(
                        outputSyntaxTree.InputPath
                        ?? throw new InvalidOperationException( "Output syntax tree has no path" ) ) ) );
        }

        return result;
    }

    private IEnumerable<string> GetDiagnosticComments( Diagnostic d )
    {
        var message = $"// {d.Severity} {d.Id} ";

        var testInputOptions = this.TestInput!.Options;

        if ( testInputOptions.IncludeLineNumberInDiagnosticReport == true )
        {
            message +=
                $"at line {d.Location.GetLineSpan().StartLinePosition.Line + 1}";
        }
        else
        {
            message += $"on `{this.GetTextUnderDiagnostic( d )}`";
        }

        if ( testInputOptions.RemoveDiagnosticMessage != true )
        {
            message += $": `{CleanMessage( d.GetMessage( CultureInfo.InvariantCulture ) )}`";
        }
        else
        {
            message += ".";
        }

        message += "\n";

        yield return message;

        foreach ( var codeFix in CodeFixTitles.GetCodeFixTitles( d ) )
        {
            yield return $"//    CodeFix: {codeFix}`\n";
        }
    }

    public void Dispose()
    {
        // This is to make sure that we have no reference to the compile-time assembly.
        // A Task<TestResult> may be non-collectable for some time after task completion (not sure why), so disposing
        // the TestResult makes sure we don't have a GC reference to the assembly even if we still have a reference to the TestResult.

        this._syntaxTrees.Clear();
        this.OutputCompilation = null;
        this.OutputProject = null;
        this.InputCompilation = null;
        this.InputProject = null;
        this.IntermediateLinkerCompilation = null;
        this.InitialCompilationModel = null;

        // Diagnostics may have reference to declarations, and must be collected too.
        this.PipelineDiagnostics.Clear();
        this.InputCompilationDiagnostics.Clear();
        this.OutputCompilationDiagnostics.Clear();
        this.CompileTimeCompilationDiagnostics.Clear();
        this.TestContext?.Dispose();
        this.TestContext = null;
    }
}