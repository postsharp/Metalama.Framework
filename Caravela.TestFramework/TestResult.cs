// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Represents the result of an integration test run.
    /// </summary>
    public class TestResult : IDiagnosticAdder
    {
        public TestInput? Input { get; set; }

        private readonly List<Diagnostic> _diagnostics = new();
        private static readonly Regex _cleanCallStackRegex = new( " in (.*):line \\d+" );

        public void Report( Diagnostic diagnostic )
        {
            this._diagnostics.Add( diagnostic );
        }

        /// <summary>
        /// Gets the list of diagnostics emitted during test run.
        /// </summary>
        public IReadOnlyList<Diagnostic> Diagnostics => this._diagnostics;

        /// <summary>
        /// Gets the primary error message emitted during test run.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Gets or sets the test project.
        /// </summary>
        public Project? Project { get; set; }

        /// <summary>
        /// Gets or sets the source code document of the template.
        /// </summary>
        public Document? TemplateDocument { get; set; }

        /// <summary>
        /// Gets or sets the initial compilation of the test project.
        /// </summary>
        public Compilation? InitialCompilation { get; set; }

        /// <summary>
        /// Gets or sets the result compilation of the test project.
        /// </summary>
        public Compilation? ResultCompilation { get; set; }

        /// <summary>
        /// Gets or sets the root <see cref="SyntaxNode"/> of the template annotated with template annotations from <see cref="AnnotationExtensions"/>.
        /// </summary>
        internal SyntaxNode? AnnotatedTemplateSyntax { get; set; }

        /// <summary>
        /// Gets the root <see cref="SyntaxNode"/> of the transformed syntax tree of the template.
        /// </summary>
        public SyntaxNode? TransformedTemplateSyntax { get; internal set; }

        public string? TransformedTemplatePath { get; internal set; }

        /// <summary>
        /// Gets the root <see cref="SyntaxNode"/> of the transformed syntax tree of the target declaration.
        /// </summary>
        public SyntaxNode? TransformedTargetSyntax { get; private set; }

        /// <summary>
        /// Gets the transformed <see cref="SourceText"/> of the target declaration.
        /// </summary>
        public SourceText? TransformedTargetSourceText { get; private set; }

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

        internal void SetTransformedTarget( SyntaxNode syntaxNode )
        {
            if ( this.InitialCompilation == null ||
                 this.Input == null ||
                 this.Project == null )
            {
                throw new InvalidOperationException( "The object has not bee properly initialized." );
            }

            string? GetTextUnderDiagnostic( Diagnostic diagnostic )
            {
                var syntaxTree = diagnostic.Location!.SourceTree;

                if ( syntaxTree == null )
                {
                    // If we don't have the a SyntaxTree, find it from the input compilation.
                    syntaxTree = this.InitialCompilation.SyntaxTrees.SingleOrDefault( t => t.FilePath == diagnostic.Location.GetLineSpan().Path );
                }

                return syntaxTree?.GetText().GetSubText( diagnostic.Location.SourceSpan ).ToString();
            }

            // Find notes annotated with [TestOutput] and choose the first one. If there is none, the test output is the whole tree
            // passed to this method.
            var outputNodes =
                syntaxNode
                    .DescendantNodesAndSelf( _ => true )
                    .OfType<MemberDeclarationSyntax>()
                    .Where(
                        m => m.AttributeLists
                            .SelectMany( list => list.Attributes )
                            .Any( a => a.Name.ToString().Contains( "TestOutput" ) ) )
                    .ToList();

            var outputNode = outputNodes.FirstOrDefault() ?? syntaxNode;

            // Convert diagnostics into comments in the code.
            var comments =
                this.Diagnostics
                    .Where(
                        d => this.Input.Options.IncludeAllSeverities.GetValueOrDefault()
                             || d.Severity >= DiagnosticSeverity.Warning )
                    .Select( d => $"// {d.Severity} {d.Id} on `{GetTextUnderDiagnostic( d )}`: `{CleanMessage( d.GetMessage() )}`\n" )
                    .OrderByDescending( s => s )
                    .Select( SyntaxFactory.Comment )
                    .ToList();

            if ( comments.Any() )
            {
                comments.Add( SyntaxFactory.LineFeed );
            }

            // Format the output code.

            var outputNodeWithComments = outputNode.WithLeadingTrivia( outputNode.GetLeadingTrivia().AddRange( comments ) );
            var formattedOutput = Formatter.Format( outputNodeWithComments, this.Project.Solution.Workspace );

            this.TransformedTargetSyntax = outputNodeWithComments;
            this.TransformedTargetSourceText = formattedOutput.GetText();
        }

        /// <summary>
        /// Gets a value indicating whether the test run succeeded.
        /// </summary>
        public bool Success { get; private set; } = true;

        /// <summary>
        /// Gets the <see cref="System.Exception"/> in which the test resulted, if any.
        /// </summary>
        public Exception? Exception { get; private set; }

        internal void SetFailed( string reason, Exception? exception = null )
        {
            this.Exception = exception;
            this.Success = false;
            this.ErrorMessage = reason;

            if ( exception != null )
            {
                this.ErrorMessage += Environment.NewLine + exception;
            }

            var emptyStatement =
                SyntaxFactory.ExpressionStatement( SyntaxFactory.IdentifierName( SyntaxFactory.MissingToken( SyntaxKind.IdentifierToken ) ) )
                    .WithSemicolonToken( SyntaxFactory.MissingToken( SyntaxKind.SemicolonToken ) );

            this.SetTransformedTarget(
                emptyStatement
                    .WithLeadingTrivia( SyntaxFactory.Comment( $"// {reason} \n" ) ) );
        }
    }
}