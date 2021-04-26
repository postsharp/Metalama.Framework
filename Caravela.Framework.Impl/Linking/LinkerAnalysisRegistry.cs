// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Contains information collected during analysis step of the linker and provides helper methods that operate on it.
    /// </summary>
    internal class LinkerAnalysisRegistry
    {
        private readonly LinkerIntroductionRegistry _introductionRegistry;
        private readonly IReadOnlyDictionary<SymbolVersion, int> _symbolVersionReferenceCounts;
        private readonly IReadOnlyDictionary<ISymbol, MemberAnalysisResult> _methodBodyInfos;
        private readonly IReadOnlyList<OrderedAspectLayer> _orderedAspectLayers;

        public LinkerAnalysisRegistry(
            LinkerIntroductionRegistry introductionRegistry,
            IReadOnlyList<OrderedAspectLayer> orderedAspectLayers,
            IReadOnlyDictionary<SymbolVersion, int> symbolVersionReferenceCounts,
            IReadOnlyDictionary<ISymbol, MemberAnalysisResult> methodBodyInfos )
        {
            this._orderedAspectLayers = orderedAspectLayers;
            this._introductionRegistry = introductionRegistry;
            this._symbolVersionReferenceCounts = symbolVersionReferenceCounts;
            this._methodBodyInfos = methodBodyInfos;
        }

        /// <summary>
        /// Determines whether the method symbol represents override target.
        /// </summary>
        /// <param name="symbol">Method symbol.</param>
        /// <returns><c>True</c> if the method is override target, otherwise <c>false</c>.</returns>
        public bool IsOverrideTarget( IMethodSymbol symbol )
        {
            return this._introductionRegistry.GetOverridesForSymbol( symbol ).Count > 0;
        }

        /// <summary>
        /// Determines whether the method body is inlineable.
        /// </summary>
        /// <param name="symbol">Method symbol.</param>
        /// <returns><c>True</c> if the method body can be inlined, otherwise <c>false</c>.</returns>
        public bool IsBodyInlineable( IMethodSymbol symbol )
        {
            if ( this.IsOverrideTarget( symbol ) )
            {
                if ( symbol.GetAttributes()
                    .Any(
                        attributeData =>
                            attributeData.AttributeClass?.ToDisplayString() == typeof(AspectLinkerOptionsAttribute).FullName
                            && attributeData.NamedArguments
                                .Any( x => x.Key == nameof(AspectLinkerOptionsAttribute.ForceNotInlineable) && (bool?) x.Value.Value == true ) ) )
                {
                    // Inlining is explicitly disabled for the method.
                    return false;
                }

                if ( !this._symbolVersionReferenceCounts.TryGetValue( new SymbolVersion( symbol, null ), out var counter ) )
                {
                    // Method is not referenced in multiple places.
                    return true;
                }

                return counter <= 1;
            }
            else if ( this.IsOverride( symbol ) )
            {
                var introducedMember = this._introductionRegistry.GetIntroducedMemberForSymbol( symbol );

                if ( introducedMember == null )
                {
                    throw new AssertionFailedException();
                }

                if ( introducedMember.LinkerOptions?.ForceNotInlineable == true )
                {
                    return false;
                }

                if ( !this._symbolVersionReferenceCounts.TryGetValue( new SymbolVersion( symbol, introducedMember.AspectLayerId ), out var counter ) )
                {
                    // This is the last override.
                    return true;
                }

                return counter <= 1;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Determines whether the symbol represents an override method.
        /// </summary>
        /// <param name="symbol">Method symbol.</param>
        /// <returns></returns>
        public bool IsOverride( IMethodSymbol symbol )
        {
            var introducedMember = this._introductionRegistry.GetIntroducedMemberForSymbol( symbol );

            if ( introducedMember == null )
            {
                return false;
            }

            return introducedMember.Semantic == IntroducedMemberSemantic.MethodOverride;
        }

        /// <summary>
        /// Gets the last (outermost) override of the method.
        /// </summary>
        /// <param name="symbol">Method symbol.</param>
        /// <returns>Symbol.</returns>
        public ISymbol GetLastOverride( IMethodSymbol symbol )
        {
            var overrides = this._introductionRegistry.GetOverridesForSymbol( symbol );
            var lastOverride = overrides.LastOrDefault();

            if ( lastOverride == null )
            {
                return symbol;
            }

            return this._introductionRegistry.GetSymbolForIntroducedMember( lastOverride );
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
            var overrides = this._introductionRegistry.GetOverridesForSymbol( (IMethodSymbol) referencedSymbol );

            var indexedLayers = this._orderedAspectLayers.Select( ( o, i ) => (o.AspectLayerId, Index: i) ).ToReadOnlyList();
            var annotationLayerIndex = indexedLayers.Single( x => x.AspectLayerId == referenceAnnotation.AspectLayer ).Index;

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

            return this._introductionRegistry.GetSymbolForIntroducedMember( previousLayerOverride );
        }

        /// <summary>
        /// Determines whether the method has a simple return control flow (i.e. if return is replaced by assignment, the control flow graph does not change).
        /// </summary>
        /// <param name="methodSymbol">Symbol.</param>
        /// <returns><c>True</c> if the body has simple control flow, otherwise <c>false</c>.</returns>
        public bool HasSimpleReturnControlFlow( IMethodSymbol methodSymbol )
        {
            if ( !this._methodBodyInfos.TryGetValue( methodSymbol, out var result ) )
            {
                return false;
            }

            return result.HasSimpleReturnControlFlow;
        }
    }
}