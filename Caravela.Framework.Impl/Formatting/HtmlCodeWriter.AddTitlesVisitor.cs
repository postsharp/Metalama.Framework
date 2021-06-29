// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.Framework.Impl.Formatting
{
    public sealed partial class HtmlCodeWriter
    {
        private class Annotator : CSharpSyntaxWalker
        {
            private readonly ClassifiedTextSpanCollection _textSpans;
            private readonly SemanticModel _semanticModel;

            public Annotator( ClassifiedTextSpanCollection textSpans, SemanticModel semanticModel ) 
            {
                this._textSpans = textSpans;
                this._semanticModel = semanticModel;
            }

            private void ProcessNode<T>( T node, TextSpan span )
                where T : SyntaxNode
            {
                if ( node.ToString() == "var" )
                {
                    return;
                }

                var symbolInfo = this._semanticModel.GetSymbolInfo( node );

                var symbol = symbolInfo.Symbol ?? this._semanticModel.GetDeclaredSymbol( node );

                if ( symbol != null )
                {
                    var doc = XmlDocumentationReader.Instance.GetFormattedDocumentation( symbol, this._semanticModel.Compilation );

                    if ( doc != null )
                    {
                        this._textSpans.SetTag( span, "title", doc );
                    }
                }
            }

            public override void VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                this.ProcessNode( node, node.Identifier.Span );
                base.VisitMethodDeclaration( node );
            }

            public override void VisitGenericName( GenericNameSyntax node ) => this.ProcessNode( node, node.Identifier.Span );

            public override void VisitIdentifierName( IdentifierNameSyntax node ) => this.ProcessNode( node, node.Span );
        }
    }
}