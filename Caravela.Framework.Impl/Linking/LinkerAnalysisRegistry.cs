// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        public static readonly SyntaxAnnotation DoNotInlineAnnotation = new( "DoNotInline" );

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
        /// Determines whether the symbol represents override target.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns><c>True</c> if the method is override target, otherwise <c>false</c>.</returns>
        public bool IsOverrideTarget( ISymbol symbol )
        {
            return this._introductionRegistry.GetOverridesForSymbol( symbol ).Count > 0;
        }

        /// <summary>
        /// Determines whether the symbol represents introduced interface implementation.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns></returns>
        public bool IsInterfaceImplementation( ISymbol symbol )
        {
            var introducedMember = this._introductionRegistry.GetIntroducedMemberForSymbol( symbol );

            if ( introducedMember == null )
            {
                return false;
            }
            else
            {
                return introducedMember.Semantic == IntroducedMemberSemantic.InterfaceImplementation;
            }
        }

        public ISymbol GetImplementedInterfaceMember( ISymbol symbol )
        {
            var introducedMember = this._introductionRegistry.GetIntroducedMemberForSymbol( symbol );

            if ( introducedMember == null || introducedMember.Semantic != IntroducedMemberSemantic.InterfaceImplementation )
            {
                throw new AssertionFailedException();
            }

            return (introducedMember.Declaration?.GetSymbol()).AssertNotNull();
        }

        /// <summary>
        /// Determines whether the method body is inlineable.
        /// </summary>
        /// <param name="symbol">Method symbol.</param>
        /// <returns><c>True</c> if the method body can be inlined, otherwise <c>false</c>.</returns>
        public bool IsInlineable( ISymbol symbol )
        {
            // TODO: Inlineability also depends on parameters passed. 
            //       Method/indexer is inlineable if only if:
            //           * Call's argument expressions match parameter names of the caller.
            //           * Parameter names of the caller match parameter names of the callee.
            //           * Caller and callee signatures are equal.
            //       This is satisfied for all proceed().

            if ( this.IsOverrideTarget( symbol ) )
            {
                // Check for the presence of a magic comment that is only used in tests.
                if ( symbol.DeclaringSyntaxReferences.Any( r => r.GetSyntax().HasAnnotation( DoNotInlineAnnotation ) ) )
                {
                    // Inlining is explicitly disabled for the declaration.
                    return false;
                }

                return this.HasSingleReference( symbol, null );
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

                return this.HasSingleReference( symbol, introducedMember.AspectLayerId );
            }
            else
            {
                // Base methods are not inlineable.

                return false;
            }
        }

        private bool HasSingleReference( ISymbol symbol, AspectLayerId? aspectLayerId )
        {
            switch ( symbol )
            {
                case IMethodSymbol { AssociatedSymbol: null }:
                    return this.HasSingleReference( symbol, aspectLayerId, LinkerAnnotationTargetKind.Self );

                case IPropertySymbol propertySymbol:
                    return this.HasSingleReference( propertySymbol, aspectLayerId, LinkerAnnotationTargetKind.PropertyGetAccessor )
                           && this.HasSingleReference( propertySymbol, aspectLayerId, LinkerAnnotationTargetKind.PropertySetAccessor );

                case IEventSymbol eventSymbol:
                    return this.HasSingleReference( eventSymbol, aspectLayerId, LinkerAnnotationTargetKind.EventAddAccessor )
                           && this.HasSingleReference( eventSymbol, aspectLayerId, LinkerAnnotationTargetKind.EventRemoveAccessor );

                default:
                    throw new NotSupportedException( $"{symbol}" );
            }
        }

        private bool HasSingleReference( ISymbol symbol, AspectLayerId? aspectLayerId, LinkerAnnotationTargetKind targetKind )
        {
            if ( !this._symbolVersionReferenceCounts.TryGetValue( new SymbolVersion( symbol, aspectLayerId, targetKind ), out var counter ) )
            {
                // Method is not referenced in multiple places.
                return true;
            }

            return counter <= 1;
        }

        /// <summary>
        /// Determines whether the symbol represents an override method.
        /// </summary>
        /// <param name="symbol">Symbol.</param>
        /// <returns></returns>
        public bool IsOverride( ISymbol symbol )
        {
            var introducedMember = this._introductionRegistry.GetIntroducedMemberForSymbol( symbol );

            if ( introducedMember == null )
            {
                return false;
            }

            return introducedMember.Semantic == IntroducedMemberSemantic.Override;
        }

        public ISymbol? GetOverrideTarget( ISymbol overrideSymbol )
        {
            var introducedMember = this._introductionRegistry.GetIntroducedMemberForSymbol( overrideSymbol );

            if ( introducedMember == null )
            {
                return null;
            }

            return this._introductionRegistry.GetOverrideTarget( introducedMember );
        }

        /// <summary>
        /// Gets the last (outermost) override of the method.
        /// </summary>
        /// <param name="symbol">Method symbol.</param>
        /// <returns>Symbol.</returns>
        public ISymbol GetLastOverride( ISymbol symbol )
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
        public ISymbol ResolveSymbolReference( ISymbol contextSymbol, ISymbol referencedSymbol, LinkerAnnotation referenceAnnotation )
        {
            // TODO: Other things than methods.
            var overrides = this._introductionRegistry.GetOverridesForSymbol( referencedSymbol );
            var indexedLayers = this._orderedAspectLayers.Select( ( o, i ) => (o.AspectLayerId, Index: i) ).ToReadOnlyList();
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
                if ( referencedSymbol is IMethodSymbol methodSymbol )
                {
                    if ( methodSymbol.OverriddenMethod != null )
                    {
                        return methodSymbol.OverriddenMethod;
                    }
                    else if ( TryGetHiddenSymbol( methodSymbol, out var hiddenSymbol ) )
                    {
                        return hiddenSymbol;
                    }
                }
                else if ( referencedSymbol is IPropertySymbol propertySymbol )
                {
                    var overridenAccessor = propertySymbol.GetMethod?.OverriddenMethod ?? propertySymbol.SetMethod?.OverriddenMethod;

                    if ( overridenAccessor != null )
                    {
                        return overridenAccessor.AssociatedSymbol.AssertNotNull();
                    }
                    else if ( TryGetHiddenSymbol( propertySymbol, out var hiddenSymbol ) )
                    {
                        return hiddenSymbol;
                    }
                }

                return referencedSymbol;
            }

            return this._introductionRegistry.GetSymbolForIntroducedMember( previousLayerOverride );
        }

        private static bool TryGetHiddenSymbol( ISymbol symbol, [NotNullWhen( true )] out ISymbol? hiddenSymbol )
        {
            var currentType = symbol.ContainingType.BaseType;

            while ( currentType != null )
            {
                // TODO: Optimize - lookup by name first instead of equating all members.
                foreach ( var member in currentType.GetMembers() )
                {
                    if ( StructuralSymbolComparer.Signature.Equals( symbol, member ) )
                    {
                        hiddenSymbol = (IMethodSymbol) member;

                        return true;
                    }
                }

                currentType = currentType.BaseType;
            }

            hiddenSymbol = null;

            return false;
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
    }
}