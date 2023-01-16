// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Elfie.Model;
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

            internal async Task<IReadOnlyList<IntermediateSymbolSemanticReference>> FindSymbolReferencesAsync<T>(
                IEnumerable<(T ReferencedSymbol, INamedTypeSymbol ReferencingType)> symbolReferenceSources,
                CancellationToken cancellationToken )
                where T : ISymbol
            {
                // TODO: Caching.
                var referencingTypes = new Dictionary<INamedTypeSymbol, HashSet<ISymbol>>( SymbolEqualityComparer.Default );

                // Currently limit the search to specified referencing types (for general call site transformations we would need to specify reference in any type).
                foreach ( var symbolReferenceSource in symbolReferenceSources)
                {
                    if (!referencingTypes.TryGetValue(symbolReferenceSource.ReferencingType, out var referencedSymbol))
                    {
                        referencingTypes[symbolReferenceSource.ReferencingType] = referencedSymbol = new HashSet<ISymbol>( SymbolEqualityComparer.Default );
                    }

                    referencedSymbol.Add( symbolReferenceSource.ReferencedSymbol );
                }

                var methodsToAnalyze = new List<(IMethodSymbol Method, HashSet<ISymbol> SymbolsToFind)>();
                var symbolReferences = new ConcurrentBag<IntermediateSymbolSemanticReference>();

                foreach ( var referencingType in referencingTypes )
                {
                    // Only take methods.
                    foreach ( var member in referencingType.Key.GetMembers() )
                    {
                        switch ( member )
                        {
                            case IMethodSymbol method:
                                methodsToAnalyze.Add( (method, referencingType.Value) );

                                break;
                        }
                    }
                }

                void Analyze( (IMethodSymbol Method, HashSet<ISymbol> SymbolsToFind) input )
                {
                    foreach ( var declaration in input.Method.DeclaringSyntaxReferences.Select( x => x.GetSyntax() ) )
                    {
                        var walker =
                            new BodyWalker(
                                this._semanticModelProvider.GetSemanticModel( declaration.SyntaxTree ),
                                input.Method,
                                input.SymbolsToFind,
                                symbolReferences );

                        walker.Visit( declaration );
                    }
                }

                await this._taskScheduler.RunInParallelAsync( methodsToAnalyze, Analyze, cancellationToken );

                return symbolReferences.ToReadOnlyList();
            }

            internal async Task<IReadOnlyList<IntermediateSymbolSemanticReference>> FindSymbolReferencesAsync(
                IEnumerable<IMethodSymbol> methodsToAnalyze,
                CancellationToken cancellationToken )
            {
                // TODO: Caching.
                var symbolReferences = new ConcurrentBag<IntermediateSymbolSemanticReference>();

                void Analyze( IMethodSymbol method )
                {
                    foreach ( var declaration in method.DeclaringSyntaxReferences.Select( x => x.GetSyntax() ) )
                    {
                        var walker =
                            new BodyWalker(
                                this._semanticModelProvider.GetSemanticModel( declaration.SyntaxTree ),
                                method,
                                null,
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
                private readonly HashSet<ISymbol>? _symbolsToFind;
                private readonly ConcurrentBag<IntermediateSymbolSemanticReference> _symbolReferences;

                public BodyWalker(
                    SemanticModel semanticModel,
                    IMethodSymbol contextSymbol,
                    HashSet<ISymbol>? symbolsToFind,
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
                             && (this._symbolsToFind == null || this._symbolsToFind.Contains( symbolInfo.Symbol ) ) )
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