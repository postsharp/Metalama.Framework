// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Templating;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Metalama.Framework.Tests.Integration.Runners
{
    internal class TestTemplateCompiler
    {
        private readonly SemanticModel _semanticModel;
        private readonly Dictionary<SyntaxNode, SyntaxNode[]> _transformedNodes = new();
        private readonly IDiagnosticAdder _diagnosticAdder;
        private readonly TemplateCompiler _templateCompiler;

        public TestTemplateCompiler( SemanticModel semanticModel, IDiagnosticAdder diagnosticAdder, IServiceProvider serviceProvider )
        {
            this._semanticModel = semanticModel;
            this._diagnosticAdder = diagnosticAdder;
            this._templateCompiler = new TemplateCompiler( serviceProvider, semanticModel.Compilation );
        }

        public bool HasError { get; private set; }

        private static bool IsTemplate( ISymbol symbol ) => symbol.GetAttributes().Any( a => a.AttributeClass?.Name == nameof(TestTemplateAttribute) );

        private bool IsTemplate( SyntaxNode node )
        {
            var symbol = this._semanticModel.GetDeclaredSymbol( node );

            if ( symbol != null )
            {
                return IsTemplate( symbol );
            }

            return false;
        }

        public bool TryCompile( CSharpCompilation compileTimeCompilation, SyntaxNode rootNode, out SyntaxNode? annotatedNode, out SyntaxNode? transformedNode )
        {
            try
            {
                var visitor = new Visitor( this, compileTimeCompilation );
                visitor.Visit( rootNode );

                annotatedNode = new Rewriter( this, 0 ).Visit( rootNode )!;
                transformedNode = new Rewriter( this, 1 ).Visit( rootNode );

                return !this.HasError;
            }
            catch ( DiagnosticException e )
            {
                this._diagnosticAdder.Report( e.Diagnostics );
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
                    var symbol = this._parent._semanticModel.GetDeclaredSymbol( node ).AssertNotNull();

                    if ( !this._parent._templateCompiler.TryCompile(
                            TemplateNameHelper.GetCompiledTemplateName( symbol ),
                            this._compileTimeCompilation,
                            node,
                            TemplateCompilerSemantics.Default,
                            this._parent._semanticModel,
                            this._parent._diagnosticAdder,
                            CancellationToken.None,
                            out var annotatedNode,
                            out var transformedNode ) )
                    {
                        this._parent.HasError = true;
                    }

                    if ( transformedNode != null )
                    {
                        var transformedTemplateText = transformedNode.ToFullString();

                        // ReSharper disable StringLiteralTypo
                        Assert.DoesNotContain( "returnglobal", transformedTemplateText, StringComparison.Ordinal );
                        Assert.DoesNotContain( "newglobal", transformedTemplateText, StringComparison.Ordinal );
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
                    return transformedNodes[this._item];
                }

                return base.Visit( node );
            }
        }
    }
}