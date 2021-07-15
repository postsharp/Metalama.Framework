// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
        private readonly IReadOnlyDictionary<ISymbol, MethodBodyAnalysisResult> _methodBodyInfos;

        private readonly
            IReadOnlyDictionary<(ISymbol Symbol, ResolvedAspectReferenceSemantic Semantic, AspectReferenceTargetKind TargetKind),
                IReadOnlyList<ResolvedAspectReference>> _aspectReferences;

        public LinkerAnalysisRegistry(
            LinkerIntroductionRegistry introductionRegistry,
            IReadOnlyDictionary<ISymbol, MethodBodyAnalysisResult> methodBodyInfos,
            IReadOnlyDictionary<(ISymbol Symbol, ResolvedAspectReferenceSemantic Semantic, AspectReferenceTargetKind TargetKind),
                IReadOnlyList<ResolvedAspectReference>> aspectReferenceIndex )
        {
            this._introductionRegistry = introductionRegistry;
            this._methodBodyInfos = methodBodyInfos;
            this._aspectReferences = aspectReferenceIndex;
        }

        /// <summary>
        /// Determines whether the symbol represents override target.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns><c>True</c> if the method is override target, otherwise <c>false</c>.</returns>
        public bool IsOverrideTarget( ISymbol symbol )
        {
            if ( symbol is IMethodSymbol methodSymbol
                 && (methodSymbol.MethodKind == MethodKind.PropertyGet
                     || methodSymbol.MethodKind == MethodKind.PropertySet
                     || methodSymbol.MethodKind == MethodKind.EventAdd
                     || methodSymbol.MethodKind == MethodKind.EventRemove) )
            {
                return this.IsOverrideTarget( methodSymbol.AssociatedSymbol.AssertNotNull() );
            }

            return this._introductionRegistry.GetOverridesForSymbol( symbol ).Count > 0;
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
            ResolvedAspectReferenceSemantic semantic,
            AspectReferenceTargetKind targetKind = AspectReferenceTargetKind.Self )
        {
            if ( !this._aspectReferences.TryGetValue( (symbol, semantic, targetKind), out var containedReferences ) )
            {
                return Array.Empty<ResolvedAspectReference>();
            }

            return containedReferences;
        }

        /// <summary>
        /// Determines whether the symbol represents an override method.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns></returns>
        public bool IsOverride( ISymbol symbol )
        {
            if ( symbol is IMethodSymbol methodSymbol
                 && (methodSymbol.MethodKind == MethodKind.PropertyGet
                     || methodSymbol.MethodKind == MethodKind.PropertySet
                     || methodSymbol.MethodKind == MethodKind.EventAdd
                     || methodSymbol.MethodKind == MethodKind.EventRemove) )
            {
                return this.IsOverride( methodSymbol.AssociatedSymbol.AssertNotNull() );
            }

            var introducedMember = this._introductionRegistry.GetIntroducedMemberForSymbol( symbol );

            if ( introducedMember == null )
            {
                return false;
            }

            return introducedMember.Semantic == IntroducedMemberSemantic.Override;
        }

        public ISymbol? GetOverrideTarget( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol { MethodKind: MethodKind.PropertyGet } getterSymbol:
                    return ((IPropertySymbol?) this.GetOverrideTarget( getterSymbol.AssociatedSymbol.AssertNotNull() ))?.GetMethod;

                case IMethodSymbol { MethodKind: MethodKind.PropertySet } setterSymbol:
                    return ((IPropertySymbol?) this.GetOverrideTarget( setterSymbol.AssociatedSymbol.AssertNotNull() ))?.SetMethod;

                case IMethodSymbol { MethodKind: MethodKind.EventAdd } adderSymbol:
                    return ((IEventSymbol?) this.GetOverrideTarget( adderSymbol.AssociatedSymbol.AssertNotNull() ))?.AddMethod;

                case IMethodSymbol { MethodKind: MethodKind.EventRemove } removerSymbol:
                    return ((IEventSymbol?) this.GetOverrideTarget( removerSymbol.AssociatedSymbol.AssertNotNull() ))?.RemoveMethod;

                default:
                    var introducedMember = this._introductionRegistry.GetIntroducedMemberForSymbol( symbol );

                    if ( introducedMember == null )
                    {
                        return null;
                    }

                    return this._introductionRegistry.GetOverrideTarget( introducedMember );
            }
        }

        /// <summary>
        /// Gets the last (outermost) override of the method.
        /// </summary>
        /// <param name="symbol">Method symbol.</param>
        /// <returns>Symbol.</returns>
        public ISymbol GetLastOverride( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IMethodSymbol { MethodKind: MethodKind.PropertyGet } getterSymbol:
                    return ((IPropertySymbol) this.GetLastOverride( getterSymbol.AssociatedSymbol.AssertNotNull() )).GetMethod.AssertNotNull();

                case IMethodSymbol { MethodKind: MethodKind.PropertySet } setterSymbol:
                    return ((IPropertySymbol) this.GetLastOverride( setterSymbol.AssociatedSymbol.AssertNotNull() )).SetMethod.AssertNotNull();

                case IMethodSymbol { MethodKind: MethodKind.EventAdd } adderSymbol:
                    return ((IEventSymbol) this.GetLastOverride( adderSymbol.AssociatedSymbol.AssertNotNull() )).AddMethod.AssertNotNull();

                case IMethodSymbol { MethodKind: MethodKind.EventRemove } removerSymbol:
                    return ((IEventSymbol) this.GetLastOverride( removerSymbol.AssociatedSymbol.AssertNotNull() )).RemoveMethod.AssertNotNull();

                default:
                    var overrides = this._introductionRegistry.GetOverridesForSymbol( symbol );
                    var lastOverride = overrides.LastOrDefault();

                    if ( lastOverride == null )
                    {
                        return symbol;
                    }

                    return this._introductionRegistry.GetSymbolForIntroducedMember( lastOverride );
            }
        }

        public bool IsLastOverride( ISymbol symbol )
        {
            return this.IsOverride( symbol ) && SymbolEqualityComparer.Default.Equals(
                symbol,
                this.GetLastOverride( this.GetOverrideTarget( symbol ).AssertNotNull() ) );
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

        internal ISymbol? GetPreviousOverride( ISymbol symbol )
        {
            var overrideTarget = this.GetOverrideTarget( symbol );

            if ( overrideTarget != null )
            {
                var overrides = this._introductionRegistry.GetOverridesForSymbol( overrideTarget );
                var matched = false;

                foreach ( var introducedMember in overrides.Reverse() )
                {
                    var overrideSymbol = this._introductionRegistry.GetSymbolForIntroducedMember( introducedMember );

                    if ( matched )
                    {
                        return overrideSymbol;
                    }

                    if ( SymbolEqualityComparer.Default.Equals( overrideSymbol, symbol ) )
                    {
                        matched = true;
                    }
                }

                return null;
            }
            else
            {
                return null;
            }
        }
    }
}