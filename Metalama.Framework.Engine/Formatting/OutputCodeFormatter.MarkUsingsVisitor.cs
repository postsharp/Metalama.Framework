// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

namespace Metalama.Framework.Engine.Formatting;

public static partial class OutputCodeFormatter
{
    private class MarkTextSpansVisitor : CSharpSyntaxWalker
    {
        private readonly ClassifiedTextSpanCollection _collection;

        public MarkTextSpansVisitor( ClassifiedTextSpanCollection collection )
        {
            this._collection = collection;
        }

        public override void DefaultVisit( SyntaxNode node )
        {
            if ( node.HasAnnotations( FormattingAnnotations.GeneratedCodeAnnotationKind ) ||
                 node.HasAnnotation( Formatter.Annotation ) )
            {
                this._collection.Add( node.FullSpan, TextSpanClassification.GeneratedCode );
            }
            else if ( node.HasAnnotation( FormattingAnnotations.SourceCodeAnnotation ) )
            {
                this._collection.Add( node.FullSpan, TextSpanClassification.SourceCode );
            }

            base.DefaultVisit( node );
        }
    }
}