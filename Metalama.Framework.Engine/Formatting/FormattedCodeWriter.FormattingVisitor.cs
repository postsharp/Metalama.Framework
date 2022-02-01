// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Formatting
{
    public partial class FormattedCodeWriter
    {
        private class FormattingVisitor : CSharpSyntaxWalker
        {
            private readonly ClassifiedTextSpanCollection _textSpans;

            public FormattingVisitor( ClassifiedTextSpanCollection textSpans ) : base( SyntaxWalkerDepth.Token )
            {
                this._textSpans = textSpans;
            }

            public override void Visit( SyntaxNode? node )
            {
                if ( node == null )
                {
                    // Coverage: ignore.
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