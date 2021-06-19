// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Represents the test results for a syntax tree in <see cref="TestResult"/>.
    /// </summary>
    public sealed class TestSyntaxTree
    {
        /// <summary>
        /// Gets the input <see cref="Document"/>.
        /// </summary>
        public Document InputDocument { get; }

        /// <summary>
        /// Gets the input <see cref="SyntaxTree"/>.
        /// </summary>
        public SyntaxTree InputSyntaxTree { get; }

        /// <summary>
        /// Gets the root <see cref="SyntaxNode"/> of the output run-time syntax tree.
        /// </summary>
        public SyntaxNode? OutputRunTimeSyntaxRoot { get; private set; }

        /// <summary>
        /// Gets the <see cref="SourceText"/> of the output run-time syntax tree.
        /// </summary>
        public SourceText? OutputRunTimeSourceText { get; private set; }

        /// <summary>
        /// Gets the root <see cref="SyntaxNode"/> of the output compile-time syntax tree.
        /// </summary>
        public SyntaxNode? OutputCompileTimeSyntaxRoot { get; private set; }

        /// <summary>
        /// Gets the root <see cref="SourceText"/> of the output compile-time syntax tree.
        /// </summary>
        public SourceText? OutputCompileTimeSourceText { get; private set; }

        /// <summary>
        /// Gets the root <see cref="SyntaxNode"/> for the annotated syntax tree (before transformation). This is
        /// useful for syntax highlighting.
        /// </summary>
        public SyntaxNode? AnnotatedSyntaxRoot { get; internal set; }

        /// <summary>
        /// Gets the full path of the code for the output compile-time syntax tree.
        /// </summary>
        public string? OutputCompileTimePath { get; private set; }

        /// <summary>
        /// Gets the full path of the HTML file with syntax highlighting.
        /// </summary>
        public string? OutputHtmlPath { get; internal set; }

        internal void SetCompileTimeCode( SyntaxNode? syntaxNode, string transformedTemplatePath )
        {
            if ( syntaxNode != null )
            {
                var formattedOutput = Formatter.Format( syntaxNode, this.InputDocument.Project.Solution.Workspace );
                this.OutputCompileTimeSyntaxRoot = syntaxNode;
                this.OutputCompileTimeSourceText = formattedOutput.GetText();
                this.OutputCompileTimePath = transformedTemplatePath;
            }
        }

        internal void SetRunTimeCode( SyntaxNode syntaxNode )
        {
            switch ( syntaxNode )
            {
                case CompilationUnitSyntax:
                case MemberDeclarationSyntax:
                    break;

                default:
                    throw new ArgumentOutOfRangeException( nameof(syntaxNode), "This node kind cannot be set as output." );
            }

            var formattedOutput = Formatter.Format( syntaxNode, this.InputDocument.Project.Solution.Workspace );
            this.OutputRunTimeSyntaxRoot = syntaxNode;
            this.OutputRunTimeSourceText = formattedOutput.GetText();
        }

        internal TestSyntaxTree( Document document )
        {
            this.InputDocument = document;
            this.InputSyntaxTree = document.GetSyntaxTreeAsync().Result!;
        }
    }
}