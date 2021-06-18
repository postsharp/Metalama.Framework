// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using System;

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

        public string OutputCompileTimePath { get; private set; }

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
                    throw new ArgumentOutOfRangeException( "This node kind cannot be set as output." );
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