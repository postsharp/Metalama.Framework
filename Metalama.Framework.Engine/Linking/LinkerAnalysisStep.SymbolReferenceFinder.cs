// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
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
            private readonly CompilationContext _compilationContext;
            private readonly IConcurrentTaskRunner _concurrentTaskRunner;
            private readonly ConcurrentDictionary<IMethodSymbol, IReadOnlyList<IntermediateSymbolSemanticReference>> _cache;

            public SymbolReferenceFinder(
                ProjectServiceProvider serviceProvider,
                CompilationContext compilationContext )
            {
                this._compilationContext = compilationContext;
                this._concurrentTaskRunner = serviceProvider.GetRequiredService<IConcurrentTaskRunner>();
                this._cache = new ConcurrentDictionary<IMethodSymbol, IReadOnlyList<IntermediateSymbolSemanticReference>>( SymbolEqualityComparer.Default );
            }

            internal async Task<IReadOnlyList<IntermediateSymbolSemanticReference>> FindSymbolReferencesAsync<T>(
                IEnumerable<(T ReferencedSymbol, INamedTypeSymbol ReferencingType)> symbolReferenceSources,
                CancellationToken cancellationToken )
                where T : ISymbol
            {
                // TODO: Caching.
                var referencingTypes = new Dictionary<INamedTypeSymbol, HashSet<ISymbol>>( this._compilationContext.SymbolComparer );

                // Currently limit the search to specified referencing types (for general call site transformations we would need to specify reference in any type).
                foreach ( var symbolReferenceSource in symbolReferenceSources )
                {
                    if ( !referencingTypes.TryGetValue( symbolReferenceSource.ReferencingType, out var referencedSymbol ) )
                    {
                        referencingTypes[symbolReferenceSource.ReferencingType] =
                            referencedSymbol = new HashSet<ISymbol>( this._compilationContext.SymbolComparer );
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
                    var references = this.AnalyzeMethod( input.Method, input.SymbolsToFind );

                    foreach ( var reference in references )
                    {
                        symbolReferences.Add( reference );
                    }
                }

                await this._concurrentTaskRunner.RunInParallelAsync( methodsToAnalyze, Analyze, cancellationToken );

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
                    var references = this.AnalyzeMethod( method );

                    foreach ( var reference in references )
                    {
                        symbolReferences.Add( reference );
                    }
                }

                await this._concurrentTaskRunner.RunInParallelAsync( methodsToAnalyze, Analyze, cancellationToken );

                return symbolReferences.ToReadOnlyList();
            }

            private IReadOnlyList<IntermediateSymbolSemanticReference> AnalyzeMethod( IMethodSymbol method, HashSet<ISymbol>? symbolsToFind = null )
            {
                var allReferences = this._cache.GetOrAdd( method, Analyze );

                if ( symbolsToFind == null )
                {
                    return allReferences;
                }
                else
                {
                    var result = new List<IntermediateSymbolSemanticReference>();

                    foreach ( var reference in allReferences )
                    {
                        if ( symbolsToFind.Contains( reference.TargetSemantic.Symbol )
                             && reference.TargetSemantic.Kind == IntermediateSymbolSemanticKind.Default )
                        {
                            result.Add( reference );
                        }
                    }

                    return result;
                }

                IReadOnlyList<IntermediateSymbolSemanticReference> Analyze( IMethodSymbol analyzedMethod )
                {
                    var symbolReferences = new List<IntermediateSymbolSemanticReference>();

                    foreach ( var declaration in analyzedMethod.DeclaringSyntaxReferences.Select( x => x.GetSyntax() ) )
                    {
                        var walker =
                            new BodyWalker(
                                this._compilationContext,
                                this._compilationContext.SemanticModelProvider.GetSemanticModel( declaration.SyntaxTree ),
                                analyzedMethod,
                                symbolReferences );

                        walker.Visit( declaration );
                    }

                    return symbolReferences;
                }
            }

            private sealed class BodyWalker : CSharpSyntaxWalker
            {
                private readonly CompilationContext _compilationContext;
                private readonly SemanticModel _semanticModel;
                private readonly IMethodSymbol _contextSymbol;
                private readonly List<IntermediateSymbolSemanticReference> _symbolReferences;

                public BodyWalker(
                    CompilationContext compilationContext,
                    SemanticModel semanticModel,
                    IMethodSymbol contextSymbol,
                    List<IntermediateSymbolSemanticReference> symbolReferences )
                {
                    this._compilationContext = compilationContext;
                    this._semanticModel = semanticModel;
                    this._contextSymbol = contextSymbol;
                    this._symbolReferences = symbolReferences;
                }

                public override void Visit( SyntaxNode? node )
                {
                    if ( node != null )
                    {
                        var symbolInfo = this._semanticModel.GetSymbolInfo( node );

                        // The search is limited to symbols declared in the type they are referenced from.

                        if ( symbolInfo.Symbol != null
                             && this._compilationContext.SymbolComparer.Equals( this._contextSymbol.ContainingType, symbolInfo.Symbol.ContainingType ) )
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