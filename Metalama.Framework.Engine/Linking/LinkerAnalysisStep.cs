// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Linking.Inlining;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Analysis step of the linker, main goal of which is to produce LinkerAnalysisRegistry.
    /// </summary>
    internal partial class LinkerAnalysisStep : AspectLinkerPipelineStep<LinkerInjectionStepOutput, LinkerAnalysisStepOutput>
    {
        private readonly IServiceProvider _serviceProvider;

        public LinkerAnalysisStep( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
        }

        public override async Task<LinkerAnalysisStepOutput> ExecuteAsync( LinkerInjectionStepOutput input, CancellationToken cancellationToken )
        {
            /*
             * Algorithm of this step:
             *  1) Collect and resolve aspect references and add implicit references (final semantic -> first override).
             *  2) Analyze reachability of semantics through aspect references, which is a DFS starting in entry point semantics, searching through all aspect references.
             *  3) Determine inlineability of reachable semantics (based on reference count).
             *  4) Determine inlineability of aspect references in pointing to inlineable semantics:
             *      * Get all inliners that can inline the reference.
             *      * If there is no inliner, reference is not inlineable.
             *      * If there is at least one inliner, reference is inlineable.
             *      * If there are multiple inliners, select one (temporarily the first one).
             *      * The selected inliner provides the principal statement.
             *  5) Inlined semantic is a semantic that is inlineable and all aspect references pointing to it are also inlineable.
             *  6) Inlined aspect reference is a aspect reference pointing to an inlined semantic.
             *  7) Analyze bodies of inlined semantics:
             *      * Collect all return statements.
             *      * Determine whether return statements are in unconditional end-points.
             *  8) Run inlining algorithm, which is DFS starting in non-inlined semantics, searching through inlined references:
             *      a) If inlined reference's replaced statement is a return statement, body is inlined without transformation of return statements.
             *      b) If inlined reference's replaced statement is NOT a return statement, all subsequent (deeper) bodies need to have return statement transformations.
             *      c) This results in having InliningSpecification for every inlineable reference.
             *  9) Create substitution objects:
             *      a) For all inlined aspect references (InliningSubstitution).  
             *      b) For all return statements that were determined to require transformation in step 8) (ReturnStatementSubstitution).
             *      c) For all implicitly returning root blocks in void methods (RootBlockSubstitution).
             *      d) For all non-inlined aspect references (AspectReferenceSubstitution).
             *  10) Create LinkerAnalysisRegistry than encapsulates all results.
             */

            var inlinerProvider = new InlinerProvider();
            var syntaxHandler = new LinkerSyntaxHandler( input.InjectionRegistry );

            var referenceResolver = new AspectReferenceResolver(
                input.InjectionRegistry,
                input.OrderedAspectLayers,
                input.FinalCompilationModel,
                input.IntermediateCompilation.Compilation );

            var aspectReferenceCollector = new AspectReferenceCollector(
                this._serviceProvider,
                input.IntermediateCompilation,
                input.InjectionRegistry,
                referenceResolver );

            var resolvedReferencesBySource = await aspectReferenceCollector.RunAsync( cancellationToken );

            var reachabilityAnalyzer = new ReachabilityAnalyzer(
                input.InjectionRegistry,
                resolvedReferencesBySource );

            var reachableSemantics = reachabilityAnalyzer.Run();

            GetReachableReferences(
                resolvedReferencesBySource,
                new HashSet<IntermediateSymbolSemantic>( reachableSemantics ),
                out var reachableReferencesByContainingSemantic,
                out var reachableReferencesByTarget );

            var inlineabilityAnalyzer = new InlineabilityAnalyzer(
                input.IntermediateCompilation,
                reachableSemantics,
                inlinerProvider,
                reachableReferencesByTarget );

            var redirectedSymbols = GetRedirectedSymbols( input.InjectionRegistry, reachableSemantics );

            var inlineableSemantics = inlineabilityAnalyzer.GetInlineableSemantics( redirectedSymbols );
            var inlineableReferences = inlineabilityAnalyzer.GetInlineableReferences( inlineableSemantics );
            var inlinedSemantics = inlineabilityAnalyzer.GetInlinedSemantics( inlineableSemantics, inlineableReferences );
            var inlinedReferences = inlineabilityAnalyzer.GetInlinedReferences( inlineableReferences, inlinedSemantics );
            var nonInlinedSemantics = reachableSemantics.Except( inlinedSemantics ).ToList();
            var nonInlinedReferencesByContainingSemantic = GetNonInlinedReferences( reachableReferencesByContainingSemantic, inlinedReferences );

            var forcefullyInitializedSymbols = GetForcefullyInitializedSymbols( input.InjectionRegistry, inlinedSemantics );
            var forcefullyInitializedTypes = GetForcefullyInitializedTypes( forcefullyInitializedSymbols );

            var bodyAnalyzer = new BodyAnalyzer(
                this._serviceProvider,
                input.IntermediateCompilation,
                reachableSemantics );

            var bodyAnalysisResults = await bodyAnalyzer.RunAsync( cancellationToken );

            var inliningAlgorithm = new InliningAlgorithm(
                this._serviceProvider,
                reachableReferencesByContainingSemantic,
                reachableSemantics,
                inlinedSemantics,
                inlinedReferences,
                bodyAnalysisResults );

            var inliningSpecifications = await inliningAlgorithm.RunAsync( cancellationToken );

            var symbolReferenceFinder = new SymbolReferenceFinder(
                this._serviceProvider,
                input.IntermediateCompilation.Compilation,
                redirectedSymbols );

            var redirectedSymbolReferences = await symbolReferenceFinder.FindSymbolReferences( redirectedSymbols.Keys, cancellationToken );

            var substitutionGenerator = new SubstitutionGenerator(
                this._serviceProvider,
                syntaxHandler,
                nonInlinedSemantics,
                nonInlinedReferencesByContainingSemantic,
                bodyAnalysisResults,
                inliningSpecifications,
                redirectedSymbols,
                redirectedSymbolReferences,
                forcefullyInitializedTypes );

            var substitutions = await substitutionGenerator.RunAsync( cancellationToken );

            var analysisRegistry = new LinkerAnalysisRegistry(
                reachableSemantics,
                inlinedSemantics,
                substitutions );

            return
                new LinkerAnalysisStepOutput(
                    input.DiagnosticSink,
                    input.IntermediateCompilation,
                    input.InjectionRegistry,
                    analysisRegistry,
                    input.ProjectOptions );
        }

        /// <summary>
        /// Gets symbols that are redirected to another semantic.
        /// </summary>
        private static IReadOnlyDictionary<ISymbol, IntermediateSymbolSemantic> GetRedirectedSymbols(
            LinkerInjectionRegistry injectionRegistry,
            IReadOnlyList<IntermediateSymbolSemantic> reachableSemantics )
        {
            var redirectedSymbols = new Dictionary<ISymbol, IntermediateSymbolSemantic>();

            foreach ( var semantic in reachableSemantics )
            {
                if ( injectionRegistry.IsOverrideTarget( semantic.Symbol )
                     && semantic.Kind == IntermediateSymbolSemanticKind.Final
                     && semantic.Symbol is IPropertySymbol { SetMethod: null, OverriddenProperty: { } } getOnlyPropertyOverride
                     && getOnlyPropertyOverride.IsAutoProperty().GetValueOrDefault() )
                {
                    // Get-only override auto property is redirected to the last override.
                    redirectedSymbols.Add(
                        semantic.Symbol,
                        injectionRegistry.GetLastOverride( semantic.Symbol ).ToSemantic( IntermediateSymbolSemanticKind.Default ) );
                }
            }

            return redirectedSymbols;
        }

        private static void GetReachableReferences(
            IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyCollection<ResolvedAspectReference>> resolvedReferencesBySource,
            HashSet<IntermediateSymbolSemantic> reachableSemantics,
            out IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> reachableReferencesBySource,
            out IReadOnlyDictionary<AspectReferenceTarget, IReadOnlyList<ResolvedAspectReference>> reachableReferencesByTarget )
        {
            var bySource = new Dictionary<IntermediateSymbolSemantic<IMethodSymbol>, List<ResolvedAspectReference>>();
            var byTarget = new Dictionary<AspectReferenceTarget, List<ResolvedAspectReference>>();

            foreach ( var pair in resolvedReferencesBySource )
            {
                // Aspect references originating in non-reachable semantics should be ignored.
                var list = new List<ResolvedAspectReference>();

                foreach ( var reference in pair.Value )
                {
                    if ( !reference.HasResolvedSemanticBody )
                    {
                        continue;
                    }

                    if ( reachableSemantics.Contains( reference.ContainingSemantic ) )
                    {
                        list.Add( reference );

                        if ( !bySource.TryGetValue( reference.ContainingSemantic, out var list2 ) )
                        {
                            bySource[reference.ContainingSemantic] = list2 = new List<ResolvedAspectReference>();
                        }

                        list2.Add( reference );

                        var target = reference.ResolvedSemantic.ToAspectReferenceTarget( reference.TargetKind );

                        if ( !byTarget.TryGetValue( target, out var list3 ) )
                        {
                            byTarget[target] = list3 = new List<ResolvedAspectReference>();
                        }

                        list3.Add( reference );
                    }
                }

                if ( list.Count > 0 )
                {
                    bySource[pair.Key] = list;
                }
            }

            reachableReferencesBySource = bySource.ToDictionary( x => x.Key, x => (IReadOnlyList<ResolvedAspectReference>) x.Value );
            reachableReferencesByTarget = byTarget.ToDictionary( x => x.Key, x => (IReadOnlyList<ResolvedAspectReference>) x.Value );
        }

        private static IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> GetNonInlinedReferences(
            IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> reachableReferencesBySource,
            IReadOnlyDictionary<ResolvedAspectReference, Inliner> inlinedReferences )
        {
            var result = new Dictionary<IntermediateSymbolSemantic<IMethodSymbol>, List<ResolvedAspectReference>>();

            foreach ( var reachableReference in reachableReferencesBySource.Values.SelectMany( x => x ) )
            {
                if ( !inlinedReferences.ContainsKey( reachableReference ) )
                {
                    if ( !result.TryGetValue( reachableReference.ContainingSemantic, out var list ) )
                    {
                        result[reachableReference.ContainingSemantic] = list = new List<ResolvedAspectReference>();
                    }

                    list.Add( reachableReference );
                }
            }

            return result.ToDictionary( x => x.Key, x => (IReadOnlyList<ResolvedAspectReference>) x.Value );
        }

        private static IReadOnlyList<ISymbol> GetForcefullyInitializedSymbols(
            LinkerInjectionRegistry injectionRegistry,
            IReadOnlyList<IntermediateSymbolSemantic> inlinedSemantics )
        {
            var forcefullyInitializedSymbols = new List<ISymbol>();

            foreach ( var semantic in inlinedSemantics )
            {
                // Currently limited to readonly structs to avoid errors.
                if ( injectionRegistry.IsOverrideTarget( semantic.Symbol )
                    && semantic.Kind == IntermediateSymbolSemanticKind.Default
                    && !semantic.Symbol.IsStatic
                    && semantic.Symbol.ContainingType is { TypeKind: TypeKind.Struct, IsReadOnly: true } )
                {
                    switch ( semantic.Symbol )
                    {
                        case IPropertySymbol property when property.IsAutoProperty() == true && !property.HasInitializer():
                            forcefullyInitializedSymbols.Add( property );
                            break;

                        case IEventSymbol @event when @event.IsEventField() && !@event.HasInitializer():
                            forcefullyInitializedSymbols.Add( @event );
                            break;
                    }
                }
            }

            return forcefullyInitializedSymbols;
        }

        private static IReadOnlyList<ForcefullyInitializedType> GetForcefullyInitializedTypes(IReadOnlyList<ISymbol> forcefullyInitializedSymbols)
        {
            var byDeclaringType = new Dictionary<INamedTypeSymbol, List<ISymbol>>( SymbolEqualityComparer.Default );

            foreach ( var symbol in forcefullyInitializedSymbols )
            {
                var declaringType = symbol.ContainingType;

                if ( !byDeclaringType.TryGetValue( declaringType, out var list ) )
                {
                    byDeclaringType[declaringType] = list = new List<ISymbol>();
                }

                list.Add( symbol );
            }

            var constructors = new Dictionary<INamedTypeSymbol, List<IntermediateSymbolSemantic<IMethodSymbol>>>( SymbolEqualityComparer.Default );

            foreach ( var type in byDeclaringType.Keys )
            {
                foreach ( var ctor in type.Constructors )
                {
                    if ( !constructors.TryGetValue( type, out var list ) )
                    {
                        constructors[type] = list = new List<IntermediateSymbolSemantic<IMethodSymbol>>();
                    }

                    list.Add( new IntermediateSymbolSemantic<IMethodSymbol>( ctor, IntermediateSymbolSemanticKind.Default ) );
                }
            }

            return constructors.Select( x => new ForcefullyInitializedType( x.Key, x.Value.ToArray(), byDeclaringType[x.Key].ToArray()) ).ToList();
        }
    }
}