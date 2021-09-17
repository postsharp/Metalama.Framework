// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Caravela.Framework.Impl.Linking
{
    internal static class RewriterExtensions
    {
        public static SyntaxTriviaList VisitTriviaList( this CSharpSyntaxRewriter rewriter, SyntaxTriviaList triviaList )
        {
            var dirty = false;
            var list = new List<SyntaxTrivia>();

            foreach ( var trivia in triviaList )
            {
                var rewrittenTrivia = rewriter.VisitTrivia( trivia );

                list.Add( rewrittenTrivia );

                if ( !trivia.Equals( rewrittenTrivia ) )
                {
                    dirty = true;
                }
            }

            if ( dirty )
            {
                return TriviaList( list );
            }
            else
            {
                return triviaList;
            }
        }
    }
}