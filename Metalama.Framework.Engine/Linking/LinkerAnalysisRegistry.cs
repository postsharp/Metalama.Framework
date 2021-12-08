// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Linking.Inlining;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Contains information collected during analysis of the intermediate assembly and provides methods querying this information.
    /// </summary>
    internal class LinkerAnalysisRegistry
    {
        private readonly IReadOnlyDictionary<ISymbol, MethodBodyAnalysisResult> _methodBodyInfos;
        private readonly HashSet<IntermediateSymbolSemantic> _reachableSymbolSemantics;
        private readonly IDictionary<IntermediateSymbolSemantic, SymbolInliningSpecification> _inliningSpecifications;

        public LinkerAnalysisRegistry(
            IReadOnlyDictionary<ISymbol, MethodBodyAnalysisResult> methodBodyInfos,
            IReadOnlyList<IntermediateSymbolSemantic> reachableSymbolSemantics,
            IReadOnlyList<SymbolInliningSpecification> inliningSpecifications )
        {
            this._methodBodyInfos = methodBodyInfos;
            this._reachableSymbolSemantics = new HashSet<IntermediateSymbolSemantic>( reachableSymbolSemantics );
            this._inliningSpecifications = inliningSpecifications.ToDictionary( x => x.Semantic, x => x );
        }

        internal IReadOnlyList<ResolvedAspectReference> GetContainedAspectReferences( IMethodSymbol symbol )
        {
            if ( this._methodBodyInfos.TryGetValue( symbol, out var methodBodyInfo ) )
            {
                return methodBodyInfo.AspectReferences;
            }

            // Coverage: ignore (irrelevant)
            return Array.Empty<ResolvedAspectReference>();
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