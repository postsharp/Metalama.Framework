// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.Engine.Linking
{
    internal static class RewriterExtensions
    {
        public static SyntaxTriviaList VisitTriviaList( this CSharpSyntaxRewriter rewriter, SyntaxTriviaList triviaList )
        {
            /* This is intended for processing trivia lists, but it is currently unused.
            var dirty = false;

#pragma warning disable IDE0059
            var list = new List<SyntaxTrivia>();

            foreach ( var trivia in triviaList )
            {
                // Not used anywhere yet. 
                throw new AssertionFailedException( Justifications.CoverageMissing );

                var rewrittenTrivia = rewriter.VisitTrivia( trivia );
                
                list.Add( rewrittenTrivia );
                
                if ( !trivia.Equals( rewrittenTrivia ) )
                {
                    dirty = true;
                }
            }

            if ( dirty )
            {
                // Not used anywhere yet. 
                throw new AssertionFailedException( Justifications.CoverageMissing );

                // return TriviaList( list );
            }
            else
            {
                return triviaList;
            }
            */

            return triviaList;
        }
    }
}