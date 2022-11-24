// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        private class SymbolReferenceFinder
        {
            private readonly ITaskScheduler _taskScheduler;
            private readonly SemanticModelProvider _semanticModelProvider;
            private readonly IReadOnlyDictionary<ISymbol, IntermediateSymbolSemantic> _redirectedSymbols;

            public SymbolReferenceFinder(
                ProjectServiceProvider serviceProvider,
                Compilation intermediateCompilation,
                IReadOnlyDictionary<ISymbol, IntermediateSymbolSemantic> redirectedSymbols )
            {
                this._taskScheduler = serviceProvider.GetRequiredService<ITaskScheduler>();
                this._semanticModelProvider = intermediateCompilation.GetSemanticModelProvider();
                this._redirectedSymbols = redirectedSymbols;
            }

            internal async Task<IReadOnlyList<IntermediateSymbolSemanticReference>> FindSymbolReferences(
                IEnumerable<ISymbol> symbols,
                CancellationToken cancellationToken )
            {
                // The search is currently limited to constructors and init-only setters.
                var containingTypes = new HashSet<INamedTypeSymbol>( SymbolEqualityComparer.Default );

                foreach ( var symbol in symbols )
                {
                    containingTypes.Add( symbol.ContainingType );
                }

                var methodsToAnalyze = new List<IMethodSymbol>();
                var symbolReferences = new ConcurrentBag<IntermediateSymbolSemanticReference>();

                foreach ( var type in containingTypes )
                {
                    foreach ( var member in type.GetMembers() )
                    {
                        switch ( member )
                        {
                            case IMethodSymbol { MethodKind: MethodKind.Constructor } constructor:
                                methodsToAnalyze.Add( constructor );

                                break;
                        }
                    }
                }

                void Analyze( IMethodSymbol method )
                {
                    foreach ( var declaration in method.DeclaringSyntaxReferences.Select( x => x.GetSyntax() ) )
                    {
                        var walker =
                            new BodyWalker(
                                this._semanticModelProvider.GetSemanticModel( declaration.SyntaxTree ),
                                method,
                                this._redirectedSymbols.Keys.ToHashSet( SymbolEqualityComparer.Default ),
                                symbolReferences );

                        walker.Visit( declaration );
                    }
                }

                await this._taskScheduler.RunInParallelAsync( methodsToAnalyze, Analyze, cancellationToken );

                return symbolReferences.ToReadOnlyList();
            }

            private class BodyWalker : CSharpSyntaxWalker
            {
                private readonly SemanticModel _semanticModel;
                private readonly IMethodSymbol _contextSymbol;
                private readonly HashSet<ISymbol> _symbolsToFind;
                private readonly ConcurrentBag<IntermediateSymbolSemanticReference> _symbolReferences;

                public BodyWalker(
                    SemanticModel semanticModel,
                    IMethodSymbol contextSymbol,
                    HashSet<ISymbol> symbolsToFind,
                    ConcurrentBag<IntermediateSymbolSemanticReference> symbolReferences )
                {
                    this._semanticModel = semanticModel;
                    this._contextSymbol = contextSymbol;
                    this._symbolsToFind = symbolsToFind;
                    this._symbolReferences = symbolReferences;
                }

                public override void Visit( SyntaxNode? node )
                {
                    if ( node != null )
                    {
                        var symbolInfo = this._semanticModel.GetSymbolInfo( node );

                        // The search is limited to symbols declared in the type they are referenced from.

                        if ( symbolInfo.Symbol != null
                             && SymbolEqualityComparer.Default.Equals( this._contextSymbol.ContainingType, symbolInfo.Symbol.ContainingType )
                             && this._symbolsToFind.Contains( symbolInfo.Symbol ) )
                        {
                            this._symbolReferences.Add(
                                new IntermediateSymbolSemanticReference(
                                    this._contextSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default ),
                                    symbolInfo.Symbol.ToSemantic( IntermediateSymbolSemanticKind.Default ),
                                    node ) );
                        }
                    }

                    base.Visit( node );
                }
            }
        }
    }
}