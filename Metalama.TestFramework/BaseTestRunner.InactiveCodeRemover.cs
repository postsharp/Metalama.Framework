// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Metalama.TestFramework
{
    public abstract partial class BaseTestRunner
    {
        /// <summary>
        /// Rewriter that removes any code made inactive by #if directives.
        /// </summary>
        private class InactiveCodeRemover : CSharpSyntaxRewriter
        {
            private readonly Stack<bool> _branchStack;

            public InactiveCodeRemover() : base( true )
            {
                this._branchStack = new Stack<bool>();
            }

            public override SyntaxTrivia VisitTrivia( SyntaxTrivia trivia )
            {
                if ( this._branchStack.Count > 0 )
                {
                    var state = this._branchStack.Peek();

                    if ( !state && (!trivia.HasStructure || (trivia.GetStructure() is not BranchingDirectiveTriviaSyntax
                                                             && trivia.GetStructure() is not EndIfDirectiveTriviaSyntax)) )
                    {
                        return SyntaxFactory.Whitespace( "" );
                    }
                }

                return base.VisitTrivia( trivia );
            }

            public override SyntaxNode? VisitIfDirectiveTrivia( IfDirectiveTriviaSyntax node )
            {
                this._branchStack.Push( node.BranchTaken );

                return null;
            }

            public override SyntaxNode? VisitElifDirectiveTrivia( ElifDirectiveTriviaSyntax node )
            {
                this._branchStack.Pop();
                this._branchStack.Push( node.BranchTaken );

                return null;
            }

            public override SyntaxNode? VisitElseDirectiveTrivia( ElseDirectiveTriviaSyntax node )
            {
                this._branchStack.Pop();
                this._branchStack.Push( node.BranchTaken );

                return null;
            }

            public override SyntaxNode? VisitEndIfDirectiveTrivia( EndIfDirectiveTriviaSyntax node )
            {
                this._branchStack.Pop();

                return null;
            }
        }
    }
}