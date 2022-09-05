// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerIntroductionRegistry
    {
        /// <summary>
        /// Discover all type and member symbols.
        /// </summary>
        private class SymbolDiscoveryWalker : SafeSyntaxWalker
        {
            private readonly SemanticModel _semanticModel;
            private readonly IDictionary<ISymbol, ISymbol> _symbolMap;

            public SymbolDiscoveryWalker(SemanticModel semanticModel, IDictionary<ISymbol, ISymbol> symbolMap)
            {
                this._semanticModel = semanticModel;
                this._symbolMap = symbolMap;
            }

            protected override void VisitCore(SyntaxNode? node)
            {
                if (node == null)
                {
                    return;
                }

                var symbol = this._semanticModel.GetDeclaredSymbol(node);

                switch ( symbol )
                {
                    case IFieldSymbol:
                    case IPropertySymbol:
                    case IEventSymbol:
                    case IMethodSymbol:
                        if (this._symbolMap.ContainsKey(symbol))
                        {
                            throw new AssertionFailedException();
                        }

                        this._symbolMap.Add( symbol, symbol );

                        break;
                }

                base.VisitCore( node );
            }

            public override void VisitBlock( BlockSyntax node )
            {
                // Not interested in descendants.
            }

            public override void VisitEqualsValueClause( EqualsValueClauseSyntax node )
            {
                // Not interested in descendants.
            }

            public override void VisitArrowExpressionClause( ArrowExpressionClauseSyntax node )
            {
                // Not interested in descendants.
            }
        }
    }
}