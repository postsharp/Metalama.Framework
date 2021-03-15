// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
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
        /// <param name="templateDocument">The source code document of the template.</param>
        internal TestResult( Document templateDocument )
        {
            this.TemplateDocument = templateDocument;
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
        public string? ErrorMessage { get; internal set; }

        /// <summary>
        /// Gets the exception thrown during test run, if any. This member is set only if <see cref="AspectTestRunner.HandlesException"/> is
        /// set to <c>true</c>.
        /// </summary>
        public Exception? Exception { get; internal set; }

        /// <summary>
        /// Gets the source code document of the template.
        /// </summary>
        public Document TemplateDocument { get; }

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
        public SyntaxNode? TransformedTargetSyntax { get; internal set; }

        /// <summary>
        /// Gets the transformed <see cref="SourceText"/> of the target code element.
        /// </summary>
        public SourceText? TransformedTargetSource { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the test run succeeded.
        /// </summary>
        public bool Success { get; internal set; }
    }
}
