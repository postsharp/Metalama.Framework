using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Linq;

namespace Caravela.TestFramework
{
    public static class TestSyntaxHelper
    {
        public static TextSpan? FindRegionSpan( SyntaxNode node, string regionName )
        {
            int regionStart = -1;
            int regionEnd = -1;
            int regionCounter = 0;
            int foundRegion = -1;

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
                            var regionDirectiveTriviaSyntax = (RegionDirectiveTriviaSyntax) trivia.GetStructure();
                            string currentRegionName = regionDirectiveTriviaSyntax.EndOfDirectiveToken.LeadingTrivia[0].ToString();
                            if ( currentRegionName.Equals( regionName, StringComparison.OrdinalIgnoreCase ) )
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
