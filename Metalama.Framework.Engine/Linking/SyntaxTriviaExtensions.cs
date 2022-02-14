// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking
{
    internal static class SyntaxTriviaExtensions
    {
        public static SyntaxTriviaList StripFirstTrailingNewLine(this SyntaxTriviaList list)
        {
            var newTrivias = new List<SyntaxTrivia>();
            var firstNewLine = true;

            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].IsKind(SyntaxKind.EndOfLineTrivia) && firstNewLine)
                {
                    firstNewLine = false;
                }
                else
                {
                    newTrivias.Add(list[i]);
                }
            }

            return SyntaxFactory.TriviaList( newTrivias );
        }
    }
}