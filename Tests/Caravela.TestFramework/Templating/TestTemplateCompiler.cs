using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.TestFramework.Templating
{
    internal class TestTemplateCompiler
    {
        private readonly TemplateCompiler _compiler = new TemplateCompiler();
        private readonly SemanticModel _semanticModel;
        private readonly Dictionary<SyntaxNode, SyntaxNode[]> _transformedNodes = new();

        public TestTemplateCompiler( SemanticModel semanticModel )
        {
            this._semanticModel = semanticModel;
        }

        public bool HasError { get; private set; }

        public List<Diagnostic> Diagnostics { get; } = new();

        public bool TryCompile( SyntaxNode rootNode, out SyntaxNode annotatedNode, out SyntaxNode transformedNode )
        {
            var visitor = new Visitor( this );
            visitor.Visit( rootNode );

            annotatedNode = new Rewriter( this, 0 ).Visit( rootNode )!;
            transformedNode = new Rewriter( this, 1 ).Visit( rootNode )!.NormalizeWhitespace();

            return !this.HasError;
        }

        private bool IsTemplate( SyntaxNode node )
        {
            var symbol = this._semanticModel.GetDeclaredSymbol( node );
            if ( symbol != null )
            {
                return this.IsTemplate( symbol );
            }
            else
            {
                return false;
            }
        }

        private bool IsTemplate( ISymbol symbol )
        {
            return symbol.GetAttributes().Any( a => a.AttributeClass?.Name == nameof( TestTemplateAttribute ) );
        }

        private class Visitor : CSharpSyntaxWalker
        {
            private readonly TestTemplateCompiler _parent;

            public Visitor( TestTemplateCompiler parent )
            {
                this._parent = parent;
            }

            public override void VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                if ( this._parent.IsTemplate( node ) )
                {
                    if ( !this._parent._compiler.TryCompile( node, this._parent._semanticModel, this._parent.Diagnostics, out var annotatedNode, out var transformedNode ) )
                    {
                        this._parent.HasError = true;
                    }

                    this._parent._transformedNodes.Add( node, new[] { annotatedNode!, transformedNode! } );
                }
            }
        }

        private class Rewriter : CSharpSyntaxRewriter
        {
            private readonly TestTemplateCompiler _parent;
            private readonly int _item;

            public Rewriter( TestTemplateCompiler parent, int item )
            {
                this._parent = parent;
                this._item = item;
            }

            public override SyntaxNode? Visit( SyntaxNode? node )
            {
                if ( node == null )
                {
                    return null;
                }

                if ( this._parent._transformedNodes.TryGetValue( node, out var transformedNodes ) )
                {
                    var transformedNode = transformedNodes[this._item];
                    if ( transformedNode != null )
                    {
                        return transformedNode;
                    }
                }

                return base.Visit( node );
            }
        }
    }
}