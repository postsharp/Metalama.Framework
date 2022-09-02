// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Utilities.Roslyn
{
    /// <summary>
    /// Rewriter that removes all preprocessor directives including inactive code.
    /// </summary>
    public class RemovePreprocessorDirectivesRewriter : SafeSyntaxRewriter
    {
        private readonly ImmutableHashSet<SyntaxKind> _preservedSyntaxKinds;
        private static readonly SyntaxTrivia _emptyTrivia = SyntaxFactory.Whitespace( "" );

        public RemovePreprocessorDirectivesRewriter( params SyntaxKind[] preservedSyntaxKinds ) : base( true )
        {
            this._preservedSyntaxKinds = ImmutableHashSet.Create( preservedSyntaxKinds );
        }

        private SyntaxNode? VisitPreprocessorDirective( SyntaxNode node ) => this._preservedSyntaxKinds.Contains( node.Kind() ) ? node : null;

        public override SyntaxTrivia VisitTrivia( SyntaxTrivia trivia )
        {
            if ( trivia.IsKind( SyntaxKind.DisabledTextTrivia ) )
            {
                return _emptyTrivia;
            }

            return base.VisitTrivia( trivia );
        }

        public override SyntaxNode? VisitIfDirectiveTrivia( IfDirectiveTriviaSyntax node ) => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitElifDirectiveTrivia( ElifDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitElseDirectiveTrivia( ElseDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitEndIfDirectiveTrivia( EndIfDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitBadDirectiveTrivia( BadDirectiveTriviaSyntax node ) => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitDefineDirectiveTrivia( DefineDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitErrorDirectiveTrivia( ErrorDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitLineDirectiveTrivia( LineDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitLoadDirectiveTrivia( LoadDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitNullableDirectiveTrivia( NullableDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitReferenceDirectiveTrivia( ReferenceDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitRegionDirectiveTrivia( RegionDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitEndRegionDirectiveTrivia( EndRegionDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitShebangDirectiveTrivia( ShebangDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitUndefDirectiveTrivia( UndefDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitWarningDirectiveTrivia( WarningDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitLineSpanDirectiveTrivia( LineSpanDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitPragmaChecksumDirectiveTrivia( PragmaChecksumDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;

        public override SyntaxNode? VisitPragmaWarningDirectiveTrivia( PragmaWarningDirectiveTriviaSyntax node )
            => node.IsActive ? this.VisitPreprocessorDirective( node ) : null;
    }
}