// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Represents the result of a test run.
    /// </summary>
    public class TestResult : IDisposable
    {
        private bool _frozen;

        public TestInput? TestInput { get; set; }

        private readonly List<TestSyntaxTree> _syntaxTrees = new();
        private static readonly Regex _cleanCallStackRegex = new( " in (.*):line \\d+" );

        public DiagnosticList InputCompilationDiagnostics { get; } = new();

        public DiagnosticList OutputCompilationDiagnostics { get; } = new();

        public DiagnosticList CompileTimeCompilationDiagnostics { get; } = new();

        public DiagnosticList PipelineDiagnostics { get; } = new();

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
        /// Gets the input test project (before transformation).
        /// </summary>
        public Project? InputProject { get; internal set; }

        /// <summary>
        /// Gets the input test project (after transformation).
        /// </summary>
        public Project? OutputProject { get; internal set; }

        /// <summary>
        /// Gets the initial compilation of the test project.
        /// </summary>
        public Compilation? InputCompilation { get; internal set; }

        /// <summary>
        /// Gets the result compilation of the test project.
        /// </summary>
        public Compilation? OutputCompilation { get; internal set; }

        /// <summary>
        /// Gets the full path of the HTML file with syntax highlighting.
        /// </summary>
        public string? OutputHtmlPath { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the test run succeeded.
        /// </summary>
        public bool Success { get; private set; } = true;

        /// <summary>
        /// Gets the <see cref="System.Exception"/> in which the test resulted, if any.
        /// </summary>
        public Exception? Exception { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output run-time code should be included in the result of
        ///  <see cref="GetConsolidatedTestOutput"/>.
        /// </summary>
        internal bool HasOutputCode { get; set; }

        public Compilation? CompileTimeCompilation { get; private set; }

        public ICompilation? InitialCompilationModel { get; internal set; }

        internal PartialCompilation? IntermediateLinkerCompilation { get; set; }

        public string? ProgramOutput { get; internal set; }

        internal void AddInputDocument( Document document, string? path ) => this._syntaxTrees.Add( new TestSyntaxTree( path, document, this ) );

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

            var i = -1;

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                if ( Path.GetFileName( syntaxTree.FilePath ) == CompileTimeConstants.PredefinedTypesFileName )
                {
                    // This is the "Intrinsics" syntax tree.
                    continue;
                }

                i++;
                var syntaxNode = await syntaxTree.GetRootAsync();

                // Format the output code.
                this.SyntaxTrees[i].SetCompileTimeCode( syntaxNode, syntaxTree.FilePath );
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

            var i = -1;

            foreach ( var syntaxTree in runTimeCompilation.SyntaxTrees )
            {
                i++;

                if ( i >= this.SyntaxTrees.Count )
                {
                    // This is the "Intrinsics" syntax tree.
                    continue;
                }

                var syntaxNode = await syntaxTree.GetRootAsync();

                // Format the output code.
                await this.SyntaxTrees[i].SetRunTimeCodeAsync( syntaxNode );
            }
        }

        internal void SetFailed( string reason, Exception? exception = null )
        {
            if ( this._frozen )
            {
                throw new InvalidOperationException( "The test result can no longer be modified." );
            }

            this._frozen = true;

            this.Exception = exception;
            this.Success = false;
            this.ErrorMessage = reason;

            if ( exception != null )
            {
                this.ErrorMessage += Environment.NewLine + exception;
            }
        }

        private string? GetTextUnderDiagnostic( Diagnostic diagnostic )
        {
            var syntaxTree = diagnostic.Location.SourceTree;

            if ( syntaxTree == null )
            {
                // If we don't have the a SyntaxTree, find it from the input compilation.
                syntaxTree = this.InputCompilation!.SyntaxTrees.SingleOrDefault( t => t.FilePath == diagnostic.Location.GetLineSpan().Path );
            }

            var text = syntaxTree?.GetText().GetSubText( diagnostic.Location.SourceSpan ).ToString();

            return text;
        }

        /// <summary>
        /// Gets the content of the <c>.t.cs</c> file, i.e. the output transformed code with comments
        /// for diagnostics.
        /// </summary>
        public CompilationUnitSyntax GetConsolidatedTestOutput()
        {
            if ( this.TestInput == null )
            {
                throw new InvalidOperationException();
            }

            var consolidatedCompilationUnit = SyntaxFactory.CompilationUnit();

            // Adding the syntax of the transformed run-time code, but only if the pipeline was successful.
            var outputSyntaxTree = this.SyntaxTrees.FirstOrDefault();

            if ( this.HasOutputCode && outputSyntaxTree is { OutputRunTimeSyntaxRoot: not null } )
            {
                var outputSyntaxRoot = FormattedCodeWriter.AddDiagnosticAnnotations(
                    outputSyntaxTree.OutputRunTimeSyntaxRoot,
                    outputSyntaxTree.InputPath,
                    this.OutputCompilationDiagnostics.Concat( this.PipelineDiagnostics ).ToArray() );

                // Find notes annotated with // <target> or with a comment containing <target> and choose the first one. If there is none, the test output is the whole tree
                // passed to this method.

                var outputNodes =
                    outputSyntaxRoot
                        .DescendantNodesAndSelf( _ => true )
                        .OfType<MemberDeclarationSyntax>()
                        .Where(
                            m => m.GetLeadingTrivia().ToString().Contains( "<target>", StringComparison.Ordinal ) ||
                                 m.AttributeLists.Any( a => a.GetLeadingTrivia().ToString().Contains( "<target>", StringComparison.Ordinal ) ) )
                        .ToList();

                var outputNode = outputNodes.FirstOrDefault() ?? (SyntaxNode) outputSyntaxRoot;

                switch ( outputNode )
                {
                    case MemberDeclarationSyntax member:
                        consolidatedCompilationUnit = consolidatedCompilationUnit.AddMembers( member.WithoutTrivia() );

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
                        throw new InvalidOperationException( $"Don't know how to add a {outputNode.Kind()} to the compilation unit." );
                }
            }

            // Adding the diagnostics as trivia.
            List<SyntaxTrivia> comments = new();

            if ( !this.Success && this.TestInput!.Options.ReportErrorMessage.GetValueOrDefault() )
            {
                comments.Add( SyntaxFactory.Comment( $"// {this.ErrorMessage} \n" ) );
            }

            // We exclude CR0222 from the results because it contains randomly-generated info and tests need to be deterministic.

            comments.AddRange(
                this.Diagnostics
                    .Where(
                        d => d.Id != "CR0222" &&
                             (this.TestInput!.Options.IncludeAllSeverities.GetValueOrDefault()
                              || d.Severity >= DiagnosticSeverity.Warning) )
                    .OrderBy( d => d.Location.SourceSpan.Start )
                    .ThenBy( d => d.GetMessage(), StringComparer.Ordinal )
                    .Select( d => $"// {d.Severity} {d.Id} on `{this.GetTextUnderDiagnostic( d )}`: `{CleanMessage( d.GetMessage() )}`\n" )
                    .Select( SyntaxFactory.Comment )
                    .ToList() );

            consolidatedCompilationUnit = consolidatedCompilationUnit.WithLeadingTrivia( comments );

            // Individual trees should be formatted, so we don't need to format again.

            return consolidatedCompilationUnit;
        }

        public void Dispose()
        {
            // This is to make sure that we have no reference to the compile-time assembly.
            // A Task<TestResult> may be non-collectable for some time after task completion (not sure why), so disposing
            // the TestResult makes sure we don't have a GC reference to the assembly even if we still have a reference to the TestResult.

            this._syntaxTrees.Clear();
            this.OutputCompilation = null;
            this.OutputCompilation = null;
            this.InputProject = null;
            this.OutputProject = null;
            this.IntermediateLinkerCompilation = null;
            this.InitialCompilationModel = null;
        }
    }
}