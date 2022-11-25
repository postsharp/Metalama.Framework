// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Metalama.Framework.CompileTimeContracts
{
    /// <summary>
    /// Represents either a <see cref="Content"/>, a <see cref="SyntaxTriviaList"/>, or <c>null</c>.
    /// </summary>
    public readonly struct StatementOrTrivia
    {
        public object? Content { get; }

        public StatementOrTrivia( StatementSyntax? statement, bool validateCode )
        {
            if ( statement != null && validateCode )
            {
                if ( statement.GetLeadingTrivia().Any( t => t.IsKind( SyntaxKind.SkippedTokensTrivia ) ) ||
                     statement.GetTrailingTrivia().Any( t => t.IsKind( SyntaxKind.SkippedTokensTrivia ) ) )
                {
                    throw new ArgumentOutOfRangeException( nameof(statement), "The code can contain a single statement." );
                }

                var missingTokens = statement.DescendantNodesAndTokens().Where( n => n.IsMissing ).ToList();

                if ( missingTokens.Any() )
                {
                    var missingToken = missingTokens.First();

                    throw new ArgumentOutOfRangeException(
                        nameof(statement),
                        $"The code is missing a {missingToken.Kind()} at position {missingToken.SpanStart + 1}." );
                }
            }

            this.Content = statement;
        }

        public StatementOrTrivia( SyntaxTriviaList triviaList )
        {
            this.Content = triviaList;
        }
    }
}