// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Formatting
{
    public partial class FormattedCodeWriter
    {
        private class FormattingVisitor : SafeSyntaxWalker
        {
            private readonly ClassifiedTextSpanCollection _textSpans;

            public FormattingVisitor( ClassifiedTextSpanCollection textSpans ) : base( SyntaxWalkerDepth.Token )
            {
                this._textSpans = textSpans;
            }

            protected override void VisitCore( SyntaxNode? node )
            {
                if ( node == null )
                {
                    // Coverage: ignore.
                    return;
                }

                if ( node.HasAnnotations( FormattingAnnotations.GeneratedCodeAnnotationKind ) )
                {
                    this._textSpans.Add( node.Span, TextSpanClassification.GeneratedCode );
                }
                else if ( node.HasAnnotation( FormattingAnnotations.SourceCodeAnnotation ) )
                {
                    this._textSpans.Add( node.Span, TextSpanClassification.SourceCode );
                }

                base.VisitCore( node );

                foreach ( var diagnosticAnnotation in node.GetAnnotations( _diagnosticAnnotationName ) )
                {
                    var span = node.Span;

                    if ( node is MethodDeclarationSyntax method )
                    {
                        span = method.Identifier.Span;
                    }

                    this._textSpans.SetTag( span, DiagnosticTagName, diagnosticAnnotation.Data! );
                }
            }
        }
    }
}