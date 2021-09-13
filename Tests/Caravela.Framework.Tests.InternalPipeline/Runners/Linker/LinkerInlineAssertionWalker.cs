// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Xunit;

namespace Caravela.Framework.Tests.Integration.Runners.Linker
{
    public class LinkerInlineAssertionWalker : CSharpSyntaxWalker
    {
        private static readonly Regex _assertionRegex = new Regex( "^[\t ]*//[\t ]*ASSERT:(?<syntax>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase );

        public override void Visit( SyntaxNode? node )
        {
            if ( node == null )
            {
                base.Visit( node );
                return;
            }

            var trivias = node.GetLeadingTrivia();

            foreach ( var trivia in trivias )
            {
                if (!trivia.HasStructure && TryParseAssertion( trivia.ToString(), out var assertedSyntax))
                {
                    var parsedAssertedTree = CSharpSyntaxTree.ParseText( assertedSyntax );
                    var parsedObservedTree = CSharpSyntaxTree.ParseText( node.ToString() );

                    Assert.Equal( parsedAssertedTree.GetRoot().NormalizeWhitespace().ToString(), parsedObservedTree.GetRoot().NormalizeWhitespace().ToString() );

                    return;
                }
            }

            base.Visit( node );
        }

        private static bool TryParseAssertion(string trivia, [NotNullWhen(true)] out string? assertedSyntax)
        {
            var match = _assertionRegex.Match( trivia );

            if (match.Success)
            {
                assertedSyntax = match.Groups["syntax"].Value;
                return true;
            }

            assertedSyntax = null;
            return false;
        }
    }
}