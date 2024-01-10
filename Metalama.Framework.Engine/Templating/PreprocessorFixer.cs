// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Templating;

// TODO: actually use this

/// <summary>
/// If, due to some manipulation of syntax tree, we end up with preprocessor directives that are not preceded by a newline,
/// the compiler will report CS1040. This class fixes that.
/// </summary>
internal static class PreprocessorFixer
{
    public static T Fix<T>( T node )
        where T : CSharpSyntaxNode
    {
        if ( !node.ContainsDirectives )
        {
            return node;
        }

        var walker = new Walker();
        walker.Visit( node );

        if ( walker.TokensToFix == null )
        {
            return node;
        }

        return node.ReplaceTokens( walker.TokensToFix, ( _, token ) => token.WithTrailingTrivia( token.TrailingTrivia.Add( SyntaxFactory.ElasticLineFeed ) ) );
    }

    private sealed class Walker : SafeSyntaxWalker
    {
        private SyntaxToken _lastToken;
        private bool _lastTriviaWasNewline;

        public List<SyntaxToken>? TokensToFix { get; private set; }

        public override void VisitToken( SyntaxToken token )
        {
            this._lastToken = token;

            base.VisitToken( token );
        }

        public override void VisitTrivia( SyntaxTrivia trivia )
        {
            if ( trivia.IsKind( SyntaxKind.EndOfLineTrivia ) )
            {
                this._lastTriviaWasNewline = true;
            }
            else if ( SyntaxFacts.IsPreprocessorDirective( trivia.Kind() ) )
            {
                if ( !this._lastTriviaWasNewline )
                {
                    this.TokensToFix ??= [];
                    this.TokensToFix.Add( this._lastToken );
                }
            }
        }
    }
}