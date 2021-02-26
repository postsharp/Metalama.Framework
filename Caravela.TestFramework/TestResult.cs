using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Represents the result of an integration test run.
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestResult"/> class.
        /// </summary>
        /// <param name="templateDocument">The source code document of the template.</param>
        public TestResult( Project project, Document templateDocument, Compilation initialCompilation )
        {
            this.Project = project;
            this.TemplateDocument = templateDocument;
            this.InitialCompilation = initialCompilation;
        }

        /// <summary>
        /// Gets or sets a list of diagnostics emitted during test run.
        /// </summary>
        public List<Diagnostic> Diagnostics { get; set; } = new List<Diagnostic>();

        /// <summary>
        /// Gets or sets a primary error message emitted during test run.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets an exception thrown during test run.
        /// </summary>
        public Exception? Exception { get; set; }

        /// <summary>
        /// Gets the test project.
        /// </summary>
        public Project Project { get; }

        /// <summary>
        /// Gets a source code document of the template.
        /// </summary>
        public Document TemplateDocument { get; }

        /// <summary>
        /// Gets an initial compilation of the test project.
        /// </summary>
        public Compilation InitialCompilation { get; }

        /// <summary>
        /// Gets or sets a result compilation of the test project.
        /// </summary>
        public Compilation? ResultCompilation { get; set; }

        /// <summary>
        /// Gets or sets an annotated syntax tree of the template.
        /// </summary>
        public SyntaxNode? AnnotatedTemplateSyntax { get; set; }

        /// <summary>
        /// Gets or sets a transformed syntax tree of the template.
        /// </summary>
        public SyntaxNode? TransformedTemplateSyntax { get; set; }

        /// <summary>
        /// Gets or sets a transformed syntax tree of the target code element.
        /// </summary>
        public SyntaxNode? TransformedTargetSyntax { get; set; }

        /// <summary>
        /// Gets or sets a transformed source of the target code element.
        /// </summary>
        public SourceText? TransformedTargetSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the test run succeeded.
        /// </summary>
        public bool Success { get; set; }
    }
}
