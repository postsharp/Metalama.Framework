// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using PostSharp.Patterns;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Represents the result of a test run.
    /// </summary>
    public class TestResult : IDiagnosticAdder
    {
        private bool _frozen;

        public TestInput? TestInput { get; set; }

        private readonly List<Diagnostic> _diagnostics = new();
        private readonly List<TestSyntaxTree> _syntaxTrees = new();
        private static readonly Regex _cleanCallStackRegex = new( " in (.*):line \\d+" );

        void IDiagnosticAdder.Report( Diagnostic diagnostic ) => this._diagnostics.Add( diagnostic );

        /// <summary>
        /// Gets the list of diagnostics emitted during test run.
        /// </summary>
        public IReadOnlyList<Diagnostic> Diagnostics => this._diagnostics;

        public IReadOnlyList<TestSyntaxTree> SyntaxTrees => this._syntaxTrees;

        /// <summary>
        /// Gets the primary error message emitted during test run.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Gets or sets the test project.
        /// </summary>
        public Project? Project { get; set; }

        /// <summary>
        /// Gets or sets the initial compilation of the test project.
        /// </summary>
        public Compilation? InputCompilation { get; set; }

        /// <summary>
        /// Gets or sets the result compilation of the test project.
        /// </summary>
        public Compilation? OutputCompilation { get; set; }

        /// <summary>
        /// Gets a value indicating whether the test run succeeded.
        /// </summary>
        public bool Success { get; private set; } = true;

        /// <summary>
        /// Gets the <see cref="System.Exception"/> in which the test resulted, if any.
        /// </summary>
        public Exception? Exception { get; private set; }

        internal void AddInputDocument( Document document ) => this._syntaxTrees.Add( new TestSyntaxTree( document ) );

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
                lines[i] = "// " + lines[i].Trim( '\r' );
            }

            return string.Join( Environment.NewLine, lines );
        }

        internal void SetOutputCompilation( Compilation runTimeCompilation )
        {
            if ( this.InputCompilation == null ||
                 this.TestInput == null ||
                 this.Project == null )
            {
                throw new InvalidOperationException( "The object has not bee properly initialized." );
            }

            var i = -1;

            foreach ( var syntaxTree in runTimeCompilation.SyntaxTrees )
            {
                i++;

                var syntaxNode = syntaxTree.GetRoot();

                // Format the output code.
                this.SyntaxTrees[i].SetRunTimeCode( syntaxNode );
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
            var syntaxTree = diagnostic.Location!.SourceTree;

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

            // Adding the syntax.
            if ( this.Success && this.SyntaxTrees.Any() )
            {
                var syntaxTree = this.SyntaxTrees.First();

                if ( syntaxTree.OutputRunTimeSyntaxRoot == null )
                {
                    throw new InvalidOperationException();
                }

                var syntaxNode = syntaxTree.OutputRunTimeSyntaxRoot!;

                // Find notes annotated with // <target> or with a comment containing <target> and choose the first one. If there is none, the test output is the whole tree
                // passed to this method.

                var outputNodes =
                    syntaxNode
                        .DescendantNodesAndSelf( _ => true )
                        .OfType<MemberDeclarationSyntax>()
                        .Where( m => m.GetLeadingTrivia().ToString().Contains( "<target>" ) )
                        .ToList();

                var outputNode = outputNodes.FirstOrDefault() ?? syntaxNode;

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
                        throw new AssertionFailedException( $"Don't know how to add a {outputNode.Kind()} to the compilation unit." );
                }
            }

            // Adding the diagnostics as trivia.
            List<SyntaxTrivia> comments = new();

            if ( !this.Success && this.TestInput!.Options.ReportErrorMessage.GetValueOrDefault() )
            {
                comments.Add( SyntaxFactory.Comment( $"// {this.ErrorMessage} \n" ) );
            }

            comments.AddRange(
                this.Diagnostics
                    .Where(
                        d => this.TestInput!.Options.IncludeAllSeverities.GetValueOrDefault()
                             || d.Severity >= DiagnosticSeverity.Warning )
                    .Select( d => $"// {d.Severity} {d.Id} on `{this.GetTextUnderDiagnostic( d )}`: `{CleanMessage( d.GetMessage() )}`\n" )
                    .OrderByDescending( s => s )
                    .Select( SyntaxFactory.Comment )
                    .ToList() );

            consolidatedCompilationUnit = consolidatedCompilationUnit.WithLeadingTrivia( comments );

            if ( this.TestInput.Options.FormatOutput.GetValueOrDefault( true ) )
            {
                var outputDocument =
                    this.Project!.RemoveDocuments( this.Project.DocumentIds.ToImmutableArray() )
                        .AddDocument( this.TestInput.TestName, consolidatedCompilationUnit );

                var simplifiedDocument = Simplifier.ReduceAsync( outputDocument ).Result;
                var simplifiedSyntaxRoot = simplifiedDocument.GetSyntaxRootAsync().Result!;
                
                consolidatedCompilationUnit = (CompilationUnitSyntax) Formatter.Format( simplifiedSyntaxRoot, this.Project!.Solution.Workspace );
            }

            return consolidatedCompilationUnit;
        }
    }
}