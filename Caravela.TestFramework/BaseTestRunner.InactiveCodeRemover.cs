// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.TestFramework
{
    public abstract partial class BaseTestRunner
    {
        /// <summary>
        /// Rewriter that removes any code made inactive by #if directives.
        /// </summary>
        private class InactiveCodeRemover : CSharpSyntaxRewriter
        {
            public InactiveCodeRemover() : base( true ) { }

            public override SyntaxTrivia VisitTrivia( SyntaxTrivia trivia )
                => trivia.Kind() == SyntaxKind.DisabledTextTrivia ? SyntaxFactory.Whitespace( "" ) : base.VisitTrivia( trivia );

            public override SyntaxNode? VisitElifDirectiveTrivia( ElifDirectiveTriviaSyntax node ) => null;

            public override SyntaxNode? VisitIfDirectiveTrivia( IfDirectiveTriviaSyntax node ) => null;

            public override SyntaxNode? VisitElseDirectiveTrivia( ElseDirectiveTriviaSyntax node ) => null;

            public override SyntaxNode? VisitEndIfDirectiveTrivia( EndIfDirectiveTriviaSyntax node ) => null;
        }
    }
}