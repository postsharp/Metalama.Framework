// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking;

internal sealed partial class LinkerAnalysisStep
{
    /// <summary>
    /// Walks syntax trees, looking for and resolving aspect references.
    /// </summary>
    private sealed class AspectReferenceWalker : SafeSyntaxWalker
    {
        private readonly AspectReferenceResolver _referenceResolver;
        private readonly Compilation _intermediateCompilation;
        private readonly SemanticModel _semanticModel;
        private readonly IMethodSymbol _containingSymbol;
        private readonly Stack<IMethodSymbol> _localFunctionStack;
        private readonly HashSet<SyntaxNode> _nodesContainingAspectReferences;

        public List<ResolvedAspectReference> AspectReferences { get; }

        public AspectReferenceWalker(
            AspectReferenceResolver referenceResolver,
            Compilation intermediateCompilation,
            SemanticModel semanticModel,
            IMethodSymbol containingSymbol,
            HashSet<SyntaxNode> nodesContainingAspectReferences )
        {
            this._intermediateCompilation = intermediateCompilation;
            this._referenceResolver = referenceResolver;
            this._semanticModel = semanticModel;
            this.AspectReferences = new List<ResolvedAspectReference>();
            this._containingSymbol = containingSymbol;
            this._localFunctionStack = new Stack<IMethodSymbol>();
            this._nodesContainingAspectReferences = nodesContainingAspectReferences;
        }

        public override void VisitLocalFunctionStatement( LocalFunctionStatementSyntax node )
        {
            var symbol = (IMethodSymbol) this._semanticModel.GetDeclaredSymbol( node ).AssertNotNull();

            try
            {
                this._localFunctionStack.Push( symbol );

                base.VisitLocalFunctionStatement( node );
            }
            finally
            {
                this._localFunctionStack.Pop();
            }
        }

        protected override void VisitCore( SyntaxNode? node )
        {
            if ( node == null )
            {
                // Coverage: ignore (irrelevant).
                return;
            }

            if ( node.TryGetAspectReference( out var aspectReference ) )
            {
                IMethodSymbol? localFunction = null;

                if ( this._localFunctionStack.Count > 0 )
                {
                    localFunction = this._localFunctionStack.Peek();
                }

                var resolvedReference = this._referenceResolver.Resolve(
                    this._containingSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default ),
                    localFunction,
                    (ExpressionSyntax) node,
                    aspectReference,
                    this._semanticModel );

                if ( resolvedReference != null )
                {
                    this.AspectReferences.Add( resolvedReference );
                }
            }

            if ( this._nodesContainingAspectReferences.Contains( node ) )
            {
                // Visit only when an aspect reference exists in the subtree of the node.
                base.VisitCore( node );
            }
        }
    }
}