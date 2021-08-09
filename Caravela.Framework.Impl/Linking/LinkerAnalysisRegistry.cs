// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking.Inlining;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Contains information collected during analysis of the intermediate assembly and provides methods querying this information.
    /// </summary>
    internal class LinkerAnalysisRegistry
    {
        private readonly LinkerIntroductionRegistry _introductionRegistry;
        private readonly IReadOnlyDictionary<ISymbol, MethodBodyAnalysisResult> _methodBodyInfos;
        private readonly IReadOnlyDictionary<AspectReferenceTarget, IReadOnlyList<ResolvedAspectReference>> _aspectReferences;
        private readonly HashSet<IntermediateSymbolSemantic> _reachableSymbolSemantics;
        private readonly IDictionary<IntermediateSymbolSemantic, SymbolInliningSpecification> _inliningSpecifications;

        public LinkerAnalysisRegistry(
            LinkerIntroductionRegistry introductionRegistry,
            IReadOnlyDictionary<ISymbol, MethodBodyAnalysisResult> methodBodyInfos,
            IReadOnlyDictionary<AspectReferenceTarget, IReadOnlyList<ResolvedAspectReference>> aspectReferenceIndex,
            IReadOnlyList<IntermediateSymbolSemantic> reachableSymbolSemantics,
            IReadOnlyList<SymbolInliningSpecification> inliningSpecifications )
        {
            this._introductionRegistry = introductionRegistry;
            this._methodBodyInfos = methodBodyInfos;
            this._aspectReferences = aspectReferenceIndex;
            this._reachableSymbolSemantics = new HashSet<IntermediateSymbolSemantic>( reachableSymbolSemantics );
            this._inliningSpecifications = inliningSpecifications.ToDictionary( x => x.Semantic, x => x );
        }

        internal IReadOnlyList<ResolvedAspectReference> GetContainedAspectReferences( IMethodSymbol symbol )
        {
            if ( this._methodBodyInfos.TryGetValue( symbol, out var methodBodyInfo ) )
            {
                return methodBodyInfo.AspectReferences;
            }

            return Array.Empty<ResolvedAspectReference>();
        }

        internal IReadOnlyList<ResolvedAspectReference> GetAspectReferences(
            ISymbol symbol,
            IntermediateSymbolSemanticKind semantic,
            AspectReferenceTargetKind targetKind = AspectReferenceTargetKind.Self )
        {
            if ( !this._aspectReferences.TryGetValue( new AspectReferenceTarget( symbol, semantic, targetKind), out var containedReferences ) )
            {
                return Array.Empty<ResolvedAspectReference>();
            }

            return containedReferences;
        }

        /// <summary>
        /// Determines whether the method has a simple return control flow (i.e. if return is replaced by assignment, the control flow graph does not change).
        /// </summary>
        /// <param name="methodSymbol">Symbol.</param>
        /// <returns><c>True</c> if the body has simple control flow, otherwise <c>false</c>.</returns>
        public bool HasSimpleReturnControlFlow( IMethodSymbol methodSymbol )
        {
            // TODO: This will go away and will be replaced by using Roslyn's control flow analysis.
            if ( !this._methodBodyInfos.TryGetValue( methodSymbol, out var result ) )
            {
                return false;
            }

            return result.HasSimpleReturnControlFlow;
        }

        public bool IsReachable( IntermediateSymbolSemantic semantic )
        {
            return this._reachableSymbolSemantics.Contains( semantic );
        }

        public bool IsInlineable( IntermediateSymbolSemantic semantic, out SymbolInliningSpecification inliningSpecification )
        {
            return this._inliningSpecifications.TryGetValue( semantic, out inliningSpecification );
        }
    }
}