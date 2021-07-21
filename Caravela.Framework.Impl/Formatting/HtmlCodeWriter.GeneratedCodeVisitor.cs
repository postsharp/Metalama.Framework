// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Formatting
{
    public sealed partial class HtmlCodeWriter
    {
        private class GeneratedCodeVisitor : CSharpSyntaxWalker
        {
            private readonly ClassifiedTextSpanCollection _textSpans;

            public GeneratedCodeVisitor( ClassifiedTextSpanCollection textSpans )
            {
                this._textSpans = textSpans;
            }

            public override void Visit( SyntaxNode? node )
            {
                if ( node == null )
                {
                    return;
                }

                if ( node.HasAnnotation( FormattingAnnotations.GeneratedCode ) )
                {
                    this._textSpans.Add( node.Span, TextSpanClassification.GeneratedCode );
                }
                else if ( node.HasAnnotation( FormattingAnnotations.SourceCode ) )
                {
                    this._textSpans.Add( node.Span, TextSpanClassification.SourceCode );
                }

                base.Visit( node );
            }
        }
    }
}