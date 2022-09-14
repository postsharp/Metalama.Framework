// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
            private readonly Compilation _compilation;
            private readonly IDictionary<ISymbol, ISymbol> _symbolMap;
            private readonly HashSet<ITypeSymbol> _visitedTypes = new HashSet<ITypeSymbol>( StructuralSymbolComparer.Default );

            private SyntaxTree? _currentSyntaxTree;
            private SemanticModel? _currentSemanticModel;

            public SymbolDiscoveryWalker(Compilation compilation, IDictionary<ISymbol, ISymbol> symbolMap) 
            {
                this._compilation = compilation;
                this._symbolMap = symbolMap;
            }

            protected override void VisitCore(SyntaxNode? node)
            {
                if (node == null)
                {
                    return;
                }

                if (node.SyntaxTree != this._currentSyntaxTree)
                {
                    this._currentSemanticModel = this._compilation.GetSemanticModel( node.SyntaxTree );
                    this._currentSyntaxTree = node.SyntaxTree;
                }

                var symbol = this._currentSemanticModel!.GetDeclaredSymbol(node);

                switch ( symbol )
                {
                    case ITypeSymbol typeSymbol:
                        // Explicitly visit all type members.
                        if ( this._visitedTypes.Add( typeSymbol ) )
                        {
                            foreach ( var member in typeSymbol.GetMembers() )
                            {
                                VisitMemberSymbol( member );
                            }
                        }

                        break;
                }

                base.VisitCore( node );

                void VisitMemberSymbol(ISymbol symbol)
                {
                    if (symbol is IMethodSymbol { IsPartialDefinition: true, PartialImplementationPart: { } partialImplementation } )
                    {
                        // For partial definitions with implementation part, we go to the implementation part and skip the definition.
                        VisitMemberSymbol( partialImplementation );
                        return;
                    }
                    
                    if ( this._symbolMap.ContainsKey( symbol ) )
                    {
                        throw new AssertionFailedException();
                    }

                    this._symbolMap.Add( symbol, symbol );
                }
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