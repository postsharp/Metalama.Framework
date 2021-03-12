// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerAnalysisRegistry
    {
        private readonly LinkerTransformationRegistry _transformationRegistry;
        private readonly Dictionary<(ISymbol Symbol, AspectLayerId? Layer), int> _symbolReferenceCounters;
        private readonly Dictionary<IMethodSymbol, (bool HasSimpleReturn, object? Dummy)> _bodyAnalysisResults;
        private readonly IReadOnlyList<OrderedAspectLayer> _orderedAspectLayers;

        private bool _frozen;

        public LinkerAnalysisRegistry(LinkerTransformationRegistry transformationRegistry, IReadOnlyList<OrderedAspectLayer> orderedAspectLayers)
        {
            this._orderedAspectLayers = orderedAspectLayers;
            this._transformationRegistry = transformationRegistry;
            this._symbolReferenceCounters = new Dictionary<(ISymbol Symbol, AspectLayerId? Layer), int>();
            this._bodyAnalysisResults = new Dictionary<IMethodSymbol, (bool, object?)>();
        }

        public void Freeze()
        {
            this._frozen = true;
        }

        public void AddReferenceCount(ISymbol symbol, AspectLayerId? referencedLayer)
        {
            if (!this._symbolReferenceCounters.TryGetValue((symbol, referencedLayer), out var count) )
            {
                count = 0;
            }

            this._symbolReferenceCounters[(symbol, referencedLayer)] = count + 1;
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
            if (this.IsOverrideTarget(symbol))
            {
                if ( symbol.GetAttributes()
                    .Any( x => 
                        x.AttributeClass?.ToDisplayString() == typeof( AspectLinkerOptionsAttribute ).FullName 
                        && x.NamedArguments
                            .Any( x => x.Key == nameof( AspectLinkerOptionsAttribute.ForceNotInlineable ) && (bool?)x.Value.Value == true ) ))
                {
                    return false;
                }

                if ( !this._symbolReferenceCounters.TryGetValue( (symbol, null), out var counter ) )
                {
                    return true;
                }

                return counter <= 1;
            }
            else
            {
                var introducedMember = this._transformationRegistry.GetIntroducedMemberForSymbol( symbol );

                if ( introducedMember == null )
                {
                    throw new AssertionFailedException();
                }

                if ( introducedMember.LinkerOptions?.ForceNotInlineable == true )
                {
                    return false;
                }

                var overrideTarget = introducedMember;
                if ( !this._symbolReferenceCounters.TryGetValue( (symbol, introducedMember.AspectLayerId), out var counter ) )
                {
                    // This is likely the last
                    return true;
                }

                return counter <= 1;
            }
        }

        public bool IsOverrideMethod( IMethodSymbol symbol )
        {
            var introducedMember = this._transformationRegistry.GetIntroducedMemberForSymbol(symbol);

            if ( introducedMember == null )
            {
                return false;
            }

            return introducedMember.Semantic == IntroducedMemberSemantic.MethodOverride;
        }

        internal ISymbol GetLastOverride( IMethodSymbol symbol )
        {
            var overrides = this._transformationRegistry.GetMethodOverridesForSymbol( symbol );
            var lastOverride = overrides.LastOrDefault();

            if ( lastOverride == null )
            {
                return symbol;
            }

            return this._transformationRegistry.GetSymbolForIntroducedMember( lastOverride );
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

            var indexedLayers = this._orderedAspectLayers.Select( ( o, i ) => (AspectLayerId: o.AspectLayerId, Index: i) );
            var annotationLayerIndex = indexedLayers.Single( x => x.AspectLayerId == referenceAnnotation.AspectLayerId ).Index;

            // TODO: Optimize.
            var previousLayerOverride = (
                from o in overrides                
                join oal in indexedLayers
                    on o.AspectLayerId equals oal.AspectLayerId
                where oal.Index < annotationLayerIndex
                orderby oal.Index
                select o
                ).LastOrDefault();

            if ( previousLayerOverride == null )
            {
                return referencedSymbol;
            }

            return this._transformationRegistry.GetSymbolForIntroducedMember( previousLayerOverride );
        }

        public bool HasSimpleReturn( IMethodSymbol methodSymbol)
        {
            if (!this._bodyAnalysisResults.TryGetValue( methodSymbol, out var result) )
            {
                return false;
            }    

            return result.HasSimpleReturn;
        }
    }
}
