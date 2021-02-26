// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerAnalysisRegistry
    {
        private bool _frozen;
        private readonly LinkerTransformationRegistry _transformationRegistry;
        private readonly Dictionary<IMethodSymbol, (bool HasSimpleReturn, object? Dummy)> _bodyAnalysisResults;
        private readonly IReadOnlyList<OrderedAspectLayer> _orderedAspectLayers;

        public LinkerAnalysisRegistry(LinkerTransformationRegistry transformationRegistry, IReadOnlyList<OrderedAspectLayer> orderedAspectLayers)
        {
            this._transformationRegistry = transformationRegistry;
        }

        public void Freeze()
        {
            this._frozen = true;
        }

        public void SetBodyAnalysisResults(IMethodSymbol symbol, bool hasSimpleReturn)
        {
            if ( this._frozen )
            {
                throw new InvalidOperationException();
            }

            this._bodyAnalysisResults.Add( symbol, (hasSimpleReturn, null) );
        }

        public bool IsOverrideTarget( IMethodSymbol symbol )
        {
            return this._transformationRegistry.GetMethodOverridesForSymbol( symbol ).Count > 0;
        }

        public bool IsBodyInlineable( IMethodSymbol symbol )
        {
            // TODO: Consume reference analysis results.
            return false;
        }

        public bool IsOverrideMethod( IMethodSymbol symbol )
        {
            var introducedMember = this._transformationRegistry.GetIntroducedMemberForSymbol(symbol);

            if ( introducedMember == null )
            {
                return false;
            }

            return introducedMember.Introductor is OverriddenMethod;
        }

        /// <summary>
        /// Resolves an annotated symbol referenced by an introduced method body, while respecting aspect layer ordering.
        /// </summary>
        /// <param name="contextSymbol">Symbol of the method body which contains the reference.</param>
        /// <param name="referencedSymbol">Symbol of the reference method (usually the original declaration).</param>
        /// <param name="referenceAnnotation">Annotation on the referencing node.</param>
        /// <returns>Symbol of the introduced declaration visible to the context method (previous aspect layer that transformed this declaration).</returns>
        public ISymbol ResolveSymbolReference( IMethodSymbol contextSymbol, ISymbol referencedSymbol, LinkerAnnotation referenceAnnotation )
        {
            // TODO: Other things than methods.
            var overrides = this._transformationRegistry.GetMethodOverridesForSymbol( (IMethodSymbol) referencedSymbol );

            // TODO: Optimize.
            var previousLayerOverride = (
                from o in overrides
                join oal in this._orderedAspectLayers on o.AspectLayerId equals oal.AspectLayerId
                where oal.Order < this._orderedAspectLayers.Single( x => x.AspectLayerId == referenceAnnotation.AspectLayerId ).Order
                orderby oal.Order
                select o
                ).Last();

            return this._transformationRegistry.GetSymbolForIntroducedMember( previousLayerOverride );
        }

        public bool HasSimpleReturn( IMethodSymbol contextSymbol )
        {
            // TODO: Consume analysis result.
            return false;
        }
    }
}
