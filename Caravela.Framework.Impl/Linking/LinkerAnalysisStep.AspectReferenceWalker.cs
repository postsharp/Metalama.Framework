// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
            private readonly SemanticModel _semanticModel;
            private readonly ISymbol _containingSymbol;

            public List<AspectReferenceHandle> AspectReferences { get; }

            public AspectReferenceWalker(SemanticModel semanticModel, ISymbol containingSymbol)
            {
                this._semanticModel = semanticModel;
                this.AspectReferences = new List<AspectReferenceHandle>();
                this._containingSymbol = containingSymbol;
            }

            public override void Visit( SyntaxNode? node )
            {
                if ( node == null )
                {
                    return;
                }

                if (node.TryGetAspectReference(out var aspectReference))
                {
                    var referencedSymbol = this._semanticModel.GetSymbolInfo(node).Symbol.AssertNotNull();
                    this.AspectReferences.Add( new AspectReferenceHandle( this._containingSymbol, referencedSymbol, (ExpressionSyntax)node, aspectReference ) );
                }

                base.Visit( node );
            }
        }
    }
}