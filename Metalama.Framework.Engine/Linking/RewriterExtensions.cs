// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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

        public static PropertyDeclarationSyntax WithSynthesizedSetter( this PropertyDeclarationSyntax propertyDeclaration )
        {
            Invariant.Assert(
                propertyDeclaration
                    .AccessorList.AssertNotNull()
                    .Accessors
                    .All( a => !a.IsKind( SyntaxKind.InitAccessorDeclaration ) && !a.IsKind( SyntaxKind.SetAccessorDeclaration ) ) );

            return propertyDeclaration
                .WithAccessorList(
                    propertyDeclaration.AccessorList.AssertNotNull()
                        .AddAccessors(
                            AccessorDeclaration(
                                SyntaxKind.InitAccessorDeclaration,
                                List<AttributeListSyntax>(),
                                propertyDeclaration.Modifiers.Any( t => t.IsKind( SyntaxKind.PrivateKeyword ) )
                                || propertyDeclaration.Modifiers.All( t => !t.IsAccessModifierKeyword() )
                                    ? TokenList()
                                    : TokenList( Token( SyntaxKind.PrivateKeyword ).WithTrailingTrivia( ElasticSpace ) ),
                                propertyDeclaration.Modifiers.Any( m => m.IsKind( SyntaxKind.StaticKeyword ) )
                                    ? Token( SyntaxKind.SetKeyword )
                                    : Token( SyntaxKind.InitKeyword ),
                                null,
                                null,
                                Token( SyntaxKind.SemicolonToken ) ) ) );
        }
    }
}