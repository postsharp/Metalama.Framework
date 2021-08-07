// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Formatting
{
    public partial class FormattedCodeWriter
    {
        private class GeneratedCodeVisitor : CSharpSyntaxWalker
        {
            private readonly ClassifiedTextSpanCollection _textSpans;

            public GeneratedCodeVisitor( ClassifiedTextSpanCollection textSpans ) : base( SyntaxWalkerDepth.Token )
            {
                this._textSpans = textSpans;
            }

            public override void VisitToken( SyntaxToken token )
            {
                if ( token.HasAnnotation( FormattingAnnotations.GeneratedCode ) )
                {
                    this._textSpans.Add( token.Span, TextSpanClassification.GeneratedCode );
                }
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

                foreach ( var diagnosticAnnotation in node.GetAnnotations( DiagnosticAnnotationName ) )
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