using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.TestFramework
{
    internal static class TestSyntaxHelper
    {
        public static TextSpan? FindRegionSpan( SyntaxNode? node, string regionName )
        {
            if ( node == null )
            {
                return null;
            }

            var regionStart = -1;
            var regionEnd = -1;
            var regionCounter = 0;
            var foundRegion = -1;

            var allRegionsTrivia = node.DescendantTrivia()
                .Where( i => i.Kind() == SyntaxKind.RegionDirectiveTrivia || i.Kind() == SyntaxKind.EndRegionDirectiveTrivia );

            foreach ( var trivia in allRegionsTrivia )
            {
                switch ( trivia.Kind() )
                {
                    case SyntaxKind.RegionDirectiveTrivia:
                        regionCounter++;
                        if ( regionStart < 0 )
                        {
                            var regionDirectiveTriviaSyntax = (RegionDirectiveTriviaSyntax?) trivia.GetStructure();
                            var currentRegionName = regionDirectiveTriviaSyntax?.EndOfDirectiveToken.LeadingTrivia[0].ToString();
                            if ( regionName.Equals( currentRegionName, StringComparison.OrdinalIgnoreCase ) )
                            {
                                foundRegion = regionCounter;
                                regionStart = trivia.Span.End;
                            }
                        }

                        break;

                    case SyntaxKind.EndRegionDirectiveTrivia:
                        if ( regionEnd < 0 )
                        {
                            if ( regionCounter == foundRegion )
                            {
                                regionEnd = trivia.Span.Start;
                            }
                        }

                        regionCounter--;
                        break;
                }
            }

            if ( regionStart >= 0 && regionEnd >= 0 )
            {
                return new TextSpan( regionStart, regionEnd - regionStart );
            }

            return null;
        }
    }
}
