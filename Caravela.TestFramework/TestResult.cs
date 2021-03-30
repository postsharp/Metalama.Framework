// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Represents the result of an integration test run.
    /// </summary>
    public sealed class TestResult
    {
        private readonly List<Diagnostic> _diagnostics = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="TestResult"/> class.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="testName"></param>
        /// <param name="templateDocument">The source code document of the template.</param>
        /// <param name="initialCompilation"></param>
        internal TestResult( Project project, string testName, Document templateDocument, Compilation initialCompilation )
        {
            this.Project = project;
            this.TestName = testName;
            this.TemplateDocument = templateDocument;
            this.InitialCompilation = initialCompilation;
        }

        internal void AddDiagnostic( Diagnostic diagnostic ) => this._diagnostics.Add( diagnostic );

        internal void AddDiagnostics( IEnumerable<Diagnostic> diagnostics ) => this._diagnostics.AddRange( diagnostics );

        /// <summary>
        /// Gets the list of diagnostics emitted during test run.
        /// </summary>
        public IReadOnlyList<Diagnostic> Diagnostics => this._diagnostics;

        /// <summary>
        /// Gets the primary error message emitted during test run.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Gets the test project.
        /// </summary>
        public Project Project { get; }

        /// <summary>
        /// Gets the short name of the test.
        /// </summary>
        public string TestName { get; }

        /// <summary>
        /// Gets the source code document of the template.
        /// </summary>
        public Document TemplateDocument { get; }

        /// <summary>
        /// Gets the initial compilation of the test project.
        /// </summary>
        public Compilation InitialCompilation { get; }

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

        /// <summary>
        /// Gets the root <see cref="SyntaxNode"/> of the transformed syntax tree of the target code element.
        /// </summary>
        public SyntaxNode? TransformedTargetSyntax { get; private set; }

        /// <summary>
        /// Gets the transformed <see cref="SourceText"/> of the target code element.
        /// </summary>
        public SourceText? TransformedTargetSourceText { get; private set; }

        internal void SetTransformedTarget( SyntaxNode syntaxNode )
        {
            static string GetTextUnderDiagnostic( Diagnostic diagnostic )
                => diagnostic.Location!.SourceTree!.GetText().GetSubText( diagnostic.Location.SourceSpan ).ToString();

            // Find notes annotated with [TestOutput] and choose the first one. If there is none, the test output is the whole tree
            // passed to this method.
            var outputNodes =
                syntaxNode
                    .DescendantNodesAndSelf( _ => true )
                    .OfType<MemberDeclarationSyntax>()
                    .Where( m => m.AttributeLists
                        .SelectMany( list => list.Attributes )
                        .Any( a => a.Name.ToString().Contains( "TestOutput" ) ) )
                    .ToList();

            var outputNode = outputNodes.FirstOrDefault() ?? syntaxNode;
        
            // Convert diagnostics into comments in the code.
            var comments =
                this.Diagnostics
                    .Where( d => !d.Id.StartsWith( "CS" ) || d.Severity >= DiagnosticSeverity.Error )
                    .Select( d => $"// {d.Severity} {d.Id} on `{GetTextUnderDiagnostic( d )}`\n" )
                    .OrderByDescending( s => s )
                    .Select( SyntaxFactory.Comment );

            if ( comments.Any() )
            {
                comments = comments.Append( SyntaxFactory.LineFeed );
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

        internal void SetFailed(string reason )
        {
            this.Success = false;
            this.ErrorMessage = reason;
            
            this.SetTransformedTarget( SyntaxFactory.EmptyStatement().WithLeadingTrivia( SyntaxFactory.Comment( "// Compilation error. Code not generated.\n" ) ));
        }
    }
}
