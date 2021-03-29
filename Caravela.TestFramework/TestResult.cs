﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using Xunit.Sdk;

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

            var comments =
                this.Diagnostics
                    .Where( d => !d.Id.StartsWith( "CS" ) )
                    .Select( d => $"// {d.Severity} {d.Id} on `{GetTextUnderDiagnostic( d )}`\n" )
                    .OrderByDescending( s => s )
                    .Select( s => SyntaxFactory.Comment( s ) );

            if ( comments.Any() )
            {
                comments = comments.Append( SyntaxFactory.LineFeed );
            }

            var syntaxNodeWithComments = syntaxNode.WithLeadingTrivia( syntaxNode.GetLeadingTrivia().AddRange( comments ) );
            var formattedOutput = Formatter.Format( syntaxNodeWithComments, this.Project.Solution.Workspace );

            this.TransformedTargetSyntax = syntaxNodeWithComments;
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

        /// <summary>
        /// Asserts that the transformed target source (i.e. the code to which the aspect is applied) is equal to an expected
        /// string. If it is different, optionally, calls a delegate.
        /// </summary>
        /// <param name="expected">The expected source code.</param>
        /// <param name="onTextDifferent">The action to execute if the text different.</param>
        public void AssertTransformedTargetCodeEqual( string expected, Action<string> onTextDifferent )
        {
            Assert.NotNull( this.TransformedTargetSourceText );

            var actual = this.TransformedTargetSourceText!.ToString().Trim();
            
            try
            {
               
                Assert.Equal( expected.Trim(), actual );
            }
            catch ( EqualException )
            {
                onTextDifferent?.Invoke( actual );

                throw;
            }
        }

        /// <summary>
        /// Asserts that the <see cref="Diagnostics"/> collection contains a diagnostic of a given id.
        /// </summary>
        /// <param name="expectedId"></param>
        public void AssertContainsDiagnosticId( string expectedId )
        {
            Assert.Contains( this.Diagnostics, d => d.Id.Equals( expectedId, StringComparison.Ordinal ) );
        }
    }
}
