// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using Caravela.Framework.Impl.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Caravela.TestFramework
{
    /// <summary>
    /// Represents the test results for a syntax tree in <see cref="TestResult"/>.
    /// </summary>
    public sealed class TestSyntaxTree
    {
        public string? InputPath { get; }

        /// <summary>
        /// Gets the input <c>Document</c>.
        /// </summary>
        public Document InputDocument { get; }

        public Document? OutputRunTimeDocument { get; private set; }

        /// <summary>
        /// Gets the input <c>SyntaxTree</c>.
        /// </summary>
        public SyntaxTree InputSyntaxTree { get; }

        /// <summary>
        /// Gets the root <c>SyntaxNode</c> of the output run-time syntax tree.
        /// </summary>
        public CompilationUnitSyntax? OutputRunTimeSyntaxRoot { get; private set; }

        /// <summary>
        /// Gets the <c>SourceText</c> of the output run-time syntax tree.
        /// </summary>
        public SourceText? OutputRunTimeSourceText { get; private set; }

        /// <summary>
        /// Gets the root <c>SyntaxNode</c> of the output compile-time syntax tree.
        /// </summary>
        public SyntaxNode? OutputCompileTimeSyntaxRoot { get; private set; }

        /// <summary>
        /// Gets the root <c>SyntaxNode</c> for the annotated syntax tree (before transformation). This is
        /// useful for syntax highlighting.
        /// </summary>
        public SyntaxNode? AnnotatedSyntaxRoot { get; internal set; }

        /// <summary>
        /// Gets the parent <see cref="TestResult"/> instance.
        /// </summary>
        public TestResult Parent { get; }

        /// <summary>
        /// Gets the full path of the code for the output compile-time syntax tree.
        /// </summary>
        public string? OutputCompileTimePath { get; private set; }

        public string? HtmlInputRunTimePath { get; internal set; }

        internal void SetCompileTimeCode( SyntaxNode? syntaxNode, string transformedTemplatePath )
        {
            if ( syntaxNode != null )
            {
                var formattedOutput = Formatter.Format( syntaxNode, this.InputDocument.Project.Solution.Workspace );
                this.OutputCompileTimeSyntaxRoot = formattedOutput;
                this.OutputCompileTimePath = transformedTemplatePath;
            }
        }

        internal async Task SetRunTimeCodeAsync( SyntaxNode syntaxNode )
        {
            CompilationUnitSyntax compilationUnit;

            switch ( syntaxNode )
            {
                case CompilationUnitSyntax cu:
                    compilationUnit = cu;

                    break;

                case MemberDeclarationSyntax member:
                    compilationUnit = SyntaxFactory.CompilationUnit().WithMembers( SyntaxFactory.SingletonList( member ) );

                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(syntaxNode),
                        $"The root of the document must be a CompilationUnitSyntax or a MemberDeclarationSyntax but it is a {syntaxNode.Kind()}." );
            }

            if ( this.Parent.OutputProject == null )
            {
                this.Parent.OutputProject = this.Parent.InputProject;
            }

            var documentName = Path.GetFileName( this.InputDocument.FilePath )!;

            var document =
                this.Parent.OutputProject!.RemoveDocument( this.InputDocument.Id )
                    .AddDocument( documentName, compilationUnit );

            if ( this.Parent.TestInput!.Options.FormatOutput.GetValueOrDefault() )
            {
                var formatted = await OutputCodeFormatter.FormatToDocumentAsync( document );

                this.OutputRunTimeDocument = formatted.Document;
                this.OutputRunTimeSyntaxRoot = formatted.Syntax;
            }
            else
            {
                this.OutputRunTimeDocument = document;
                this.OutputRunTimeSyntaxRoot = compilationUnit;
            }

            this.OutputRunTimeSourceText = await (await this.OutputRunTimeDocument.GetSyntaxTreeAsync())!.GetTextAsync();
        }

        internal TestSyntaxTree( string? inputPath, Document document, TestResult parent )
        {
            this.InputDocument = document;
            this.Parent = parent;
            this.InputPath = inputPath;
            this.InputSyntaxTree = document.GetSyntaxTreeAsync().Result!;
        }
    }
}