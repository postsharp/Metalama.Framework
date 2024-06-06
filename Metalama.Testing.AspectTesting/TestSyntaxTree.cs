// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Formatting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Metalama.Testing.AspectTesting
{
    /// <summary>
    /// Represents the test results for a syntax tree in <see cref="TestResult"/>.
    /// </summary>
    internal sealed class TestSyntaxTree
    {
        private readonly TestResult _parent;

        private TestSyntaxTree( string? inputPath, Document? inputDocument, TestResult parent, SyntaxTree? inputSyntaxTree )
        {
            this.InputDocument = inputDocument;
            this._parent = parent;
            this.InputPath = inputPath;
            this.InputSyntaxTree = inputSyntaxTree;
        }

        public static async Task<TestSyntaxTree> CreateAsync( string? inputPath, Document document, TestResult parent )
        {
            var syntaxTree = await document.GetSyntaxTreeAsync();

            return new TestSyntaxTree( inputPath, document, parent, syntaxTree.AssertNotNull() );
        }

        public static TestSyntaxTree CreateIntroduced( TestResult parent )
        {
            return new TestSyntaxTree( null, null, parent, null );
        }

        public bool IsAuxiliary => this.InputPath != null && Path.GetFileName( this.InputPath ).StartsWith( "_", StringComparison.Ordinal );

        /// <summary>
        /// Gets the file path of the syntax tree. For input syntax trees, this is an absolute path of the input document.
        /// For introduced syntax trees, this is a relative path under which the syntax tree was introduced. 
        /// </summary>
        public string FilePath => this.InputPath ?? this.OutputDocument?.FilePath ?? throw new InvalidOperationException("The test syntax tree does not have a path.");

        /// <summary>
        /// Gets the input path from which the syntax tree was loaded. For introduced syntax trees, this is <c>null</c>.
        /// </summary>
        public string? InputPath { get; }

        /// <summary>
        /// Gets the input <see cref="Document" />. For introduced syntax trees, this is <c>null</c>.
        /// </summary>
        public Document? InputDocument { get; }

        /// <summary>
        /// Gets the output document.
        /// </summary>
        public Document? OutputDocument { get; private set; }

        /// <summary>
        /// Gets the input <see cref="SyntaxTree" />. For introduced syntax trees, this is <c>null</c>.
        /// </summary>
        public SyntaxTree? InputSyntaxTree { get; }

        /// <summary>
        /// Gets the root <see cref="SyntaxNode" /> of the output run-time syntax tree.
        /// </summary>
        public CompilationUnitSyntax? OutputRunTimeSyntaxRoot { get; private set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// Gets the root <see cref="SyntaxNode" /> of the output compile-time syntax tree.
        /// </summary>
        [UsedImplicitly]
        public SyntaxNode? OutputCompileTimeSyntaxRoot { get; private set; }

        /// <summary>
        /// Gets or sets the root <see cref="SyntaxNode" /> for the annotated syntax tree (before transformation). This is
        /// useful for syntax highlighting.
        /// </summary>
        public SyntaxNode? AnnotatedSyntaxRoot { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global

        /// <summary>
        /// Gets the full path of the code for the output compile-time syntax tree.
        /// </summary>
        [UsedImplicitly]
        public string? OutputCompileTimePath { get; private set; }

        public string? HtmlInputPath { get; internal set; }

        public string? HtmlOutputPath { get; internal set; }

        internal void SetCompileTimeCode( SyntaxNode? syntaxNode, string transformedTemplatePath )
        {
            if ( syntaxNode != null )
            {
                if ( this.InputDocument == null )
                {
                    throw new AssertionFailedException( "Introduced syntax trees cannot have compile-time code." );
                }

                var formattedOutput = Formatter.Format( syntaxNode, this.InputDocument.Project.Solution.Workspace );
                this.OutputCompileTimeSyntaxRoot = formattedOutput;
                this.OutputCompileTimePath = transformedTemplatePath;
            }
        }

        public async Task SetRunTimeCodeAsync( SyntaxNode syntaxNode )
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

            this._parent.OutputProject ??= this._parent.InputProject;

            Document outputDocument;

            if ( this.InputDocument != null )
            {
                var documentName = Path.GetFileName( this.InputDocument.FilePath )!;

                outputDocument =
                    this._parent.OutputProject!.RemoveDocument( this.InputDocument.Id )
                        .AddDocument( documentName, compilationUnit );

                this._parent.OutputProject = outputDocument.Project;
            }
            else
            {
                outputDocument = this._parent.OutputProject!.AddDocument(
                    syntaxNode.SyntaxTree.FilePath,
                    compilationUnit,
                    filePath: syntaxNode.SyntaxTree.FilePath );

                this._parent.OutputProject = outputDocument.Project;
            }

            if ( this._parent.TestInput!.Options.FormatOutput.GetValueOrDefault() )
            {
                var codeFormatter = new CodeFormatter();

                this.OutputDocument = await codeFormatter.FormatAsync( outputDocument );
                this.OutputRunTimeSyntaxRoot = (CompilationUnitSyntax) (await this.OutputDocument.GetSyntaxRootAsync())!;
            }
            else
            {
                this.OutputDocument = outputDocument;
                this.OutputRunTimeSyntaxRoot = compilationUnit;
            }
        }
    }
}