// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Templating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Tests.Integration.Templating
{
    internal class TestTemplateCompiler
    {
        private readonly TemplateCompiler _compiler = new();
        private readonly SemanticModel _semanticModel;
        private readonly Dictionary<SyntaxNode, SyntaxNode[]> _transformedNodes = new();

        public TestTemplateCompiler( SemanticModel semanticModel )
        {
            this._semanticModel = semanticModel;
        }

        public bool HasError { get; private set; }

        public List<Diagnostic> Diagnostics { get; } = new();

        private static bool IsTemplate( ISymbol symbol )
        {
            return symbol.GetAttributes().Any( a => a.AttributeClass?.Name == nameof( TestTemplateAttribute ) );
        }

        private bool IsTemplate( SyntaxNode node )
        {
            var symbol = this._semanticModel.GetDeclaredSymbol( node );
            if ( symbol != null )
            {
                return IsTemplate( symbol );
            }
            else
            {
                return false;
            }
        }

        public bool TryCompile( CSharpCompilation compileTimeCompilation, SyntaxNode rootNode, out SyntaxNode? annotatedNode, out SyntaxNode? transformedNode )
        {
            try
            {
                var visitor = new Visitor( this, compileTimeCompilation );
                visitor.Visit( rootNode );

                annotatedNode = new Rewriter( this, 0 ).Visit( rootNode )!;
                transformedNode = new Rewriter( this, 1 ).Visit( rootNode )!.NormalizeWhitespace();

                return !this.HasError;
            }
            catch ( InvalidUserCodeException e )
            {
                this.Diagnostics.AddRange( e.Diagnostics );
                annotatedNode = null;
                transformedNode = null;
                return false;
            }
        }

        private class Visitor : CSharpSyntaxWalker
        {
            private readonly TestTemplateCompiler _parent;
            private readonly Compilation _compileTimeCompilation;

            public Visitor( TestTemplateCompiler parent, Compilation compileTimeCompilation )
            {
                this._parent = parent;
                this._compileTimeCompilation = compileTimeCompilation;
            }

            public override void VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                if ( this._parent.IsTemplate( node ) )
                {
                    if ( !this._parent._compiler.TryCompile( this._compileTimeCompilation, node, this._parent._semanticModel, this._parent.Diagnostics, out var annotatedNode, out var transformedNode ) )
                    {
                        this._parent.HasError = true;
                    }

                    this._parent._transformedNodes.Add( node, new SyntaxNode[] { annotatedNode!, transformedNode! } );
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
                    return transformedNodes[this._item];
                }

                return base.Visit( node );
            }
        }
    }
}