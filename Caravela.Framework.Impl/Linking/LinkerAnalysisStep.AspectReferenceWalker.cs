// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Aspects;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerAnalysisStep
    {
        // TODO: Change this to counting return statements that change the control flow.

        /// <summary>
        /// Walks method bodies, counting return statements.
        /// </summary>
        private class AspectReferenceWalker : CSharpSyntaxWalker
        {
            private readonly AspectReferenceResolver _referenceResolver;
            private readonly SemanticModel _semanticModel;
            private readonly ISymbol _containingSymbol;

            public List<ResolvedAspectReference> AspectReferences { get; }

            public AspectReferenceWalker( AspectReferenceResolver referenceResolver, SemanticModel semanticModel, ISymbol containingSymbol )
            {
                this._referenceResolver = referenceResolver;
                this._semanticModel = semanticModel;
                this.AspectReferences = new List<ResolvedAspectReference>();
                this._containingSymbol = containingSymbol;
            }

            public override void Visit( SyntaxNode? node )
            {
                if ( node == null )
                {
                    return;
                }

                if ( node.TryGetAspectReference( out var aspectReference ) )
                {
                    var nodeWithSymbol = node switch
                    {
                        ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.WhenNotNull,
                        _ => node
                    };

                    var referencedSymbol = this._semanticModel.GetSymbolInfo( nodeWithSymbol ).Symbol.AssertNotNull();

                    var resolvedReference = this._referenceResolver.Resolve(
                        this._containingSymbol,
                        referencedSymbol,
                        (ExpressionSyntax) node,
                        aspectReference );

                    this.AspectReferences.Add( resolvedReference );
                }

                base.Visit( node );
            }
        }
    }
}