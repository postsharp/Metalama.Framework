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
    public sealed class TestSyntaxTree
    {
        public Document InputDocument { get; }
        
        public SyntaxTree InputSyntaxTree { get; }
        
        public SyntaxNode? OutputRunTimeSyntaxRoot { get; private set; }
        
        public SourceText? OutputRunTimeSourceText { get; private set; }
        
        public SyntaxNode? OutputCompileTimeSyntaxRoot { get; private set; }
        
        public SourceText? OutputCompileTimeSourceText { get; private set; }
        
        public SyntaxNode? AnnotatedSyntaxRoot { get; internal set; }

        public string OutputCompileTimePath { get; private  set; }

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

        internal void SetRunTimeCode(SyntaxNode syntaxNode )
        {
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
    
    /// <summary>
    /// Represents the result of a test run.
    /// </summary>
    public class TestResult : IDiagnosticAdder
    {
        public TestInput? TestInput { get; set; }

        private readonly List<Diagnostic> _diagnostics = new();
        private readonly List<TestSyntaxTree> _syntaxTrees = new();
        private static readonly Regex _cleanCallStackRegex = new( " in (.*):line \\d+" );

        public void Report( Diagnostic diagnostic )
        {
            this._diagnostics.Add( diagnostic );
        }

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


        public void AddInputDocument( Document document )
        {
            this._syntaxTrees.Add( new TestSyntaxTree( document ) );
        }

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

        internal void SetTransformedCompilation( Compilation runTimeCompilation )
        {
            if ( this.InputCompilation == null ||
                 this.TestInput == null ||
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
                    syntaxTree = this.InputCompilation.SyntaxTrees.SingleOrDefault( t => t.FilePath == diagnostic.Location.GetLineSpan().Path );
                }

                return syntaxTree?.GetText().GetSubText( diagnostic.Location.SourceSpan ).ToString();
            }

            var i = -1;
            
            foreach ( var syntaxTree in runTimeCompilation.SyntaxTrees )
            {
                i++;
                
                var syntaxNode = syntaxTree.GetRoot();

                // Find notes annotated with // <target> or with a comment containing <target> and choose the first one. If there is none, the test output is the whole tree
                // passed to this method.
                var outputNodes =
                    syntaxNode
                        .DescendantNodesAndSelf( _ => true )
                        .OfType<MemberDeclarationSyntax>()
                        .Where( m => m.GetLeadingTrivia().ToString().Contains( "<target>" ) )
                        .ToList();

                var outputNode = outputNodes.FirstOrDefault() ?? syntaxNode;

                // Convert diagnostics into comments in the code.
                if ( i == 0 )
                {
                    var comments =
                        this.Diagnostics
                            .Where(
                                d => this.TestInput.Options.IncludeAllSeverities.GetValueOrDefault()
                                     || d.Severity >= DiagnosticSeverity.Warning )
                            .Select( d => $"// {d.Severity} {d.Id} on `{GetTextUnderDiagnostic( d )}`: `{CleanMessage( d.GetMessage() )}`\n" )
                            .OrderByDescending( s => s )
                            .Select( SyntaxFactory.Comment )
                            .ToList();

                    if ( comments.Any() )
                    {
                        comments.Add( SyntaxFactory.LineFeed );
                        outputNode = outputNode.WithLeadingTrivia( comments );
                    }
                }

                // Format the output code.
                this.SyntaxTrees[i].SetRunTimeCode( outputNode );
                
            }
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

            this.SyntaxTrees.First()
                .SetRunTimeCode(
                    emptyStatement
                        .WithLeadingTrivia( SyntaxFactory.Comment( $"// {reason} \n" ) ) );

        }

    }
}