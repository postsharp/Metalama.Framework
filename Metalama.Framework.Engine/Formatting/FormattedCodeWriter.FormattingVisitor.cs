// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.Engine.Formatting
{
    public partial class FormattedCodeWriter
    {
        private sealed class FormattingVisitor : ClassifierBase
        {
            private bool _visitTrivia;

            public FormattingVisitor( ClassifiedTextSpanCollection textSpans ) : base( textSpans ) { }

            protected override void VisitCore( SyntaxNode? node )
            {
                if ( node == null )
                {
                    // Coverage: ignore.
                    return;
                }

                if ( !this._visitTrivia && !node.ContainsAnnotations )
                {
                    return;
                }

                var setVisitTrivia = false;

                if ( node.HasAnnotations( FormattingAnnotations.GeneratedCodeAnnotationKind ) )
                {
                    this.ClassifiedTextSpans.Add( node.Span, TextSpanClassification.GeneratedCode );
                    setVisitTrivia = true;
                }
                else if ( node.HasAnnotation( FormattingAnnotations.SourceCodeAnnotation ) )
                {
                    this.ClassifiedTextSpans.Add( node.Span, TextSpanClassification.SourceCode );
                }

                if ( setVisitTrivia )
                {
                    var previousVisitTrivia = this._visitTrivia;
                    this._visitTrivia = true;
                    try
                    {
                        base.VisitCore( node );
                    }
                    finally
                    {
                        this._visitTrivia = previousVisitTrivia;
                    }
                }
                else
                {
                    base.VisitCore( node );
                }

                foreach ( var diagnosticAnnotation in node.GetAnnotations( _diagnosticAnnotationName ) )
                {
                    var span = node.Span;

                    if ( node is MethodDeclarationSyntax method )
                    {
                        span = method.Identifier.Span;
                    }

                    this.ClassifiedTextSpans.SetTag( span, DiagnosticTagName, diagnosticAnnotation.Data! );
                }
            }

            public override void VisitToken( SyntaxToken token )
            {
                if ( this._visitTrivia )
                {
                    base.VisitToken( token );
                }
            }
        }
    }
}