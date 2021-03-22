// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.Templating
{
    internal class ProceedCallAnnotator : CSharpSyntaxRewriter
    {
        private readonly SemanticAnnotationMap _semanticAnnotationMap;
        private int _calls;

        public ProceedCallAnnotator( SemanticAnnotationMap semanticAnnotationMap )
        {
            this._semanticAnnotationMap = semanticAnnotationMap;
        }

        public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

        public override SyntaxNode? Visit( SyntaxNode? node )
        {

            var transformedNode = base.Visit( node );

            if ( transformedNode == null )
            {
                return null;
            }

            if ( transformedNode.ChildNodes().Any( n => n.HasCallsProceedAnnotation() ) )
            {
                return transformedNode.AddCallsProceedAnnotation();
            }
            else
            {
                return transformedNode;
            }
        }

        private bool IsProceed( SyntaxNode node )
        {
            var symbol = this._semanticAnnotationMap.GetSymbol( node );

            if ( symbol == null )
            {
                return false;
            }

            return symbol.GetAttributes().Any( a => a.AttributeClass?.Name == nameof( ProceedAttribute ) );
        }

        public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
        {
            if ( this.IsProceed( node.Expression ) )
            {
                if ( this._calls > 0 )
                {
                    this.Diagnostics.Add( 
                        Diagnostic.Create( 
                            "CA07", 
                            "Annotation",
                            $"The {node} method cannot be called twice in a template method.",
                            DiagnosticSeverity.Error,
                            DiagnosticSeverity.Error, 
                            true, 
                            0,
                            location: Location.Create( node.SyntaxTree, node.Span ) ) );
                }

                this._calls++;
                return node.AddCallsProceedAnnotation();
            }
            else
            {
                return base.VisitInvocationExpression( node );
            }
        }
    }
}