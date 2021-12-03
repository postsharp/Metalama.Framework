// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Reflection;

namespace Caravela.Framework.Impl.Templating
{
    [Obfuscation( Exclude = true )]
    public readonly struct StatementOrTrivia
    {
        public object? Statement { get; }

        internal StatementOrTrivia( StatementSyntax? statement, bool validateCode )
        {
            if ( statement != null && validateCode )
            {
                if ( statement.GetLeadingTrivia().Any( t => t.IsKind( SyntaxKind.SkippedTokensTrivia ) ) ||
                     statement.GetTrailingTrivia().Any( t => t.IsKind( SyntaxKind.SkippedTokensTrivia ) ) )
                {
                    throw new ArgumentOutOfRangeException( nameof( statement ), "The code can contain a single statement." );
                }

                var missingTokens = statement.DescendantNodesAndTokens().Where( n => n.IsMissing ).ToList();

                if ( missingTokens.Any() )
                {
                    var missingToken = missingTokens.First();

                    throw new ArgumentOutOfRangeException(
                        nameof( statement ),
                        $"The code is missing a {missingToken.Kind()} at position {missingToken.SpanStart + 1}." );
                }
            }

            this.Statement = statement;
        }

        internal StatementOrTrivia( SyntaxTriviaList statement )
        {
            this.Statement = statement;
        }
    }
}