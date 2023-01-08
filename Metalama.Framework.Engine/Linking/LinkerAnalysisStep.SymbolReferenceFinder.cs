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
    internal sealed partial class LinkerAnalysisStep
    {
        private sealed class SymbolReferenceFinder
        {
            private readonly ITaskScheduler _taskScheduler;
            private readonly SemanticModelProvider _semanticModelProvider;

            public SymbolReferenceFinder(
                ProjectServiceProvider serviceProvider,
                Compilation intermediateCompilation )
            {
                this._taskScheduler = serviceProvider.GetRequiredService<ITaskScheduler>();
                this._semanticModelProvider = intermediateCompilation.GetSemanticModelProvider();
            }

            internal async Task<IReadOnlyList<IntermediateSymbolSemanticReference>> FindSymbolReferencesAsync(
                IEnumerable<ISymbol> symbols,
                CancellationToken cancellationToken )
            {
                // TODO: Caching.
                // The search is currently limited to constructors and init-only setters.
                var containingTypes = new HashSet<INamedTypeSymbol>( SymbolEqualityComparer.Default );
                var symbolsToFind = symbols.ToHashSet();

                // Currently limit the search to declaring types (this would need to change for general call site transformations).
                foreach ( var symbol in symbolsToFind )
                {
                    containingTypes.Add( symbol.ContainingType );
                }

                var methodsToAnalyze = new List<IMethodSymbol>();
                var symbolReferences = new ConcurrentBag<IntermediateSymbolSemanticReference>();

                foreach ( var type in containingTypes )
                {
                    // Only take methods.
                    foreach ( var member in type.GetMembers() )
                    {
                        switch ( member )
                        {
                            case IMethodSymbol method:
                                methodsToAnalyze.Add( method );

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
                                symbolsToFind,
                                symbolReferences );

                        walker.Visit( declaration );
                    }
                }

                await this._taskScheduler.RunInParallelAsync( methodsToAnalyze, Analyze, cancellationToken );

                return symbolReferences.ToReadOnlyList();
            }

            private sealed class BodyWalker : CSharpSyntaxWalker
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