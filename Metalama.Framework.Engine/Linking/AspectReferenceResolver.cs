﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

// Ordered declaration versions (intermediate compilation):
//  * Overridden declaration (base class declaration)
//  * Target declaration, base semantic (if from source code)
//  * Target declaration, default semantic (if introduced, no overridden declaration)
//  * Override 1-1
//  ...
//  * Override z-1
//  ...
//  * Override z-k (there are multiple overrides of the same declaration on one layer).
//  ...
//  * Override n
//  * Target declaration,final semantic)

// Each of above correspond to an aspect layer in the global order.
// The reference we are trying to resolve also originates in of the aspect layers.

// Declaration semantics projected to global aspect layer order:
// * Layer (0, 0):   Overridden declaration (base class declaration).
// * Layer (0, 0):   Target declaration, default semantic (if from source code).
// * Layer (0, 0):   Target declaration, base semantic (if introduced, no overridden declaration).
// ...
// * Layer (k, 0):   Target declaration, default semantic (if introduced).
// * Layer (k, 1):   After override 1-1 (same layer as introduction).
// ...
// * Layer (l_1, 1): After override 2-1 (layer with multiple overrides).
// ...
// * Layer (l_1, k): After override 2-k.
// ...
// * Layer (l_n, 1): After override n.
// ...
// * Layer (m, 0):   Target declaration, final semantic.

// AspectReferenceOrder resolution:
//  * Original - resolves to the first in the order.
//  * Base - resolved to the last override preceding the origin layer.
//  * Self - resolved to the last override preceding or equal to the origin layer.
//  * Final - resolved to the last in the order.

// Special cases:
//  * Promoted fields do not count as introductions. The layer of the promotion target applies.
//    Source promoted fields are treated as source declarations. Introduced and then promoted fields
//    are treated as being introduced at the point of field introduction.

// Notes:
//  * Base and Self are different only for layers that override the referenced declaration.

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Resolves aspect references.
    /// </summary>
    internal class AspectReferenceResolver
    {
        private readonly LinkerInjectionRegistry _injectionRegistry;
        private readonly IReadOnlyList<AspectLayerId> _orderedLayers;
        private readonly IReadOnlyDictionary<AspectLayerId, int> _layerIndex;
        private readonly CompilationModel _finalCompilationModel;
        private readonly Compilation _intermediateCompilation;

        public AspectReferenceResolver(
            LinkerInjectionRegistry injectionRegistry,
            IReadOnlyList<OrderedAspectLayer> orderedAspectLayers,
            CompilationModel finalCompilationModel,
            Compilation intermediateCompilation )
        {
            this._injectionRegistry = injectionRegistry;

            var indexedLayers =
                new[] { AspectLayerId.Null }
                    .Concat( orderedAspectLayers.Select( x => x.AspectLayerId ) )
                    .Select( ( al, i ) => (AspectLayerId: al, Index: i) )
                    .ToList();

            this._orderedLayers = indexedLayers.Select( x => x.AspectLayerId ).ToReadOnlyList();
            this._layerIndex = indexedLayers.ToDictionary( x => x.AspectLayerId, x => x.Index );
            this._finalCompilationModel = finalCompilationModel;
            this._intermediateCompilation = intermediateCompilation;
        }

        public ResolvedAspectReference Resolve(
            IntermediateSymbolSemantic<IMethodSymbol> containingSemantic,
            ISymbol referencedSymbol,
            ExpressionSyntax expression,
            AspectReferenceSpecification referenceSpecification,
            SemanticModel semanticModel )
        {
            // Get the local symbol that is referenced and reference root.
            // E.g. explicit interface implementation must be referenced as interface member reference.
            ResolveTarget(
                containingSemantic.Symbol,
                referencedSymbol,
                expression,
                semanticModel,
                out var resolvedRootNode,
                out var resolvedReferencedSymbol,
                out var resolvedReferencedSymbolSourceNode );

            var targetKind = referenceSpecification.TargetKind;
            var isInlineable = (referenceSpecification.Flags & AspectReferenceFlags.Inlineable) != 0;

            if ( targetKind == AspectReferenceTargetKind.Self && resolvedReferencedSymbol is IPropertySymbol or IEventSymbol or IFieldSymbol )
            {
                // Resolves the symbol based on expression - this is used when aspect reference targets property/event/field
                // but it is not specified whether the getter/setter/adder/remover is targeted.
                targetKind = ResolveExpressionTarget( resolvedReferencedSymbol, expression );
            }

            // At this point we should always target a method or a specific target.
            Invariant.AssertNot( resolvedReferencedSymbol is IPropertySymbol or IEventSymbol && targetKind == AspectReferenceTargetKind.Self );

            var annotationLayerIndex = this.GetAnnotationLayerIndex( containingSemantic.Symbol, resolvedReferencedSymbol, referenceSpecification );

            // If the override target was introduced, determine the index.
            var targetIntroductionInjectedMember = this._injectionRegistry.GetInjectedMemberForSymbol( resolvedReferencedSymbol );
            var targetIntroductionIndex = this.GetIntroductionLogicalIndex( targetIntroductionInjectedMember );

            var overrideIndices = this.GetOverrideIndices( resolvedReferencedSymbol );

            this.ResolveLayerIndex(
                referenceSpecification,
                annotationLayerIndex,
                targetIntroductionInjectedMember,
                targetIntroductionIndex,
                overrideIndices,
                out var resolvedIndex,
                out var resolvedInjectedMember );

            if ( resolvedReferencedSymbol is IFieldSymbol field )
            {
                // Field symbols are resolved to themselves (this may be temporary).
                var fieldSemantic =
                    targetIntroductionInjectedMember == null
                        ? IntermediateSymbolSemanticKind.Default
                        : resolvedIndex < targetIntroductionIndex
                            ? IntermediateSymbolSemanticKind.Base
                            : IntermediateSymbolSemanticKind.Default;

                return new ResolvedAspectReference(
                    containingSemantic,
                    resolvedReferencedSymbol,
                    field.ToSemantic( fieldSemantic ),
                    expression,
                    resolvedRootNode,
                    resolvedReferencedSymbolSourceNode,
                    targetKind,
                    isInlineable );
            }

            // At this point resolvedIndex should be 0, equal to target introduction index, this._orderedLayers.Count or be equal to index of one of the overrides.
            Invariant.Assert(
                resolvedIndex == default
                || resolvedIndex == new MemberLayerIndex( this._orderedLayers.Count, 0 )
                || overrideIndices.Any( x => x.Index == resolvedIndex )
                || resolvedIndex == targetIntroductionIndex );

            if ( overrideIndices.Count > 0 && resolvedIndex == overrideIndices[overrideIndices.Count - 1].Index )
            {
                // If we have resolved to the last override, transition to the final declaration index.
                resolvedIndex = new MemberLayerIndex( this._orderedLayers.Count, 0 );
            }

            if ( resolvedIndex == default )
            {
                if ( targetIntroductionInjectedMember == null )
                {
                    // There is no introduction, i.e. this is a user source symbol.
                    return new ResolvedAspectReference(
                        containingSemantic,
                        resolvedReferencedSymbol,
                        resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default ),
                        expression,
                        resolvedRootNode,
                        resolvedReferencedSymbolSourceNode,
                        targetKind,
                        isInlineable );
                }
                else
                {
                    // There is an introduction and this reference points to a state before that introduction.
                    if ( referencedSymbol.IsOverride )
                    {
                        // Introduction is an override, resolve to symbol in the base class.
                        return new ResolvedAspectReference(
                            containingSemantic,
                            resolvedReferencedSymbol,
                            GetOverriddenSymbol( resolvedReferencedSymbol ).AssertNotNull().ToSemantic( IntermediateSymbolSemanticKind.Default ),
                            expression,
                            resolvedRootNode,
                            resolvedReferencedSymbolSourceNode,
                            targetKind,
                            isInlineable );
                    }
                    else if ( targetIntroductionInjectedMember.Transformation is IReplaceMemberTransformation { ReplacedMember: { } replacedMember }
                              && replacedMember.GetTarget( this._finalCompilationModel, ReferenceResolutionOptions.DoNotFollowRedirections )
                                  .GetSymbol() != null )
                    {
                        // Introduction replaced existing source member, resolve to default semantics, i.e. source symbol.

                        return new ResolvedAspectReference(
                            containingSemantic,
                            resolvedReferencedSymbol,
                            resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default ),
                            expression,
                            resolvedRootNode,
                            resolvedReferencedSymbolSourceNode,
                            targetKind,
                            isInlineable );
                    }
                    else
                    {
                        // Introduction is a new member, resolve to base semantics, i.e. the base method.
                        return new ResolvedAspectReference(
                            containingSemantic,
                            resolvedReferencedSymbol,
                            resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Base ),
                            expression,
                            resolvedRootNode,
                            resolvedReferencedSymbolSourceNode,
                            targetKind,
                            isInlineable );
                    }
                }
            }
            else if ( resolvedIndex == targetIntroductionIndex )
            {
                // We have resolved to the target member introduction.
                // The only way to get here is using "Base" order in the first override.
                if ( HasImplicitImplementation( resolvedReferencedSymbol ) )
                {
                    return new ResolvedAspectReference(
                        containingSemantic,
                        resolvedReferencedSymbol,
                        resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default ),
                        expression,
                        resolvedRootNode,
                        resolvedReferencedSymbolSourceNode,
                        targetKind,
                        isInlineable );
                }
                else
                {
                    if ( resolvedReferencedSymbol.IsOverride )
                    {
                        // Introduction is an override, resolve to the symbol in the base class.
                        return new ResolvedAspectReference(
                            containingSemantic,
                            resolvedReferencedSymbol,
                            GetOverriddenSymbol( resolvedReferencedSymbol ).AssertNotNull().ToSemantic( IntermediateSymbolSemanticKind.Default ),
                            expression,
                            resolvedRootNode,
                            resolvedReferencedSymbolSourceNode,
                            targetKind,
                            isInlineable );
                    }
                    else if ( this.TryGetHiddenSymbol( resolvedReferencedSymbol, out var hiddenSymbol ) )
                    {
                        // The introduction is hiding another member, resolve to default semantics.
                        return new ResolvedAspectReference(
                            containingSemantic,
                            resolvedReferencedSymbol,
                            hiddenSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default ),
                            expression,
                            resolvedRootNode,
                            resolvedReferencedSymbolSourceNode,
                            targetKind,
                            isInlineable );
                    }
                    else
                    {
                        // Introduction is a new member, resolve to base semantics, i.e. the empty method from the builder.
                        return new ResolvedAspectReference(
                            containingSemantic,
                            resolvedReferencedSymbol,
                            resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Base ),
                            expression,
                            resolvedRootNode,
                            resolvedReferencedSymbolSourceNode,
                            targetKind,
                            isInlineable );
                    }
                }
            }
            else if ( resolvedIndex.LayerIndex < this._orderedLayers.Count )
            {
                // One of the overrides or the introduced member.
                if ( targetIntroductionInjectedMember != null && resolvedIndex.MemberIndex == 0 )
                {
                    // TODO: This would happen has the introduced member contained aspect reference. Bodies of introduced members are
                    //       currently not used.
                    throw new AssertionFailedException( Justifications.CoverageMissing );

                    // // There is no introduction, i.e. this is a user source symbol.
                    // return new ResolvedAspectReference(
                    //     containingSymbol,
                    //     referencedSymbol,
                    //     new IntermediateSymbolSemantic(
                    //         this.GetSymbolFromInjectedMember( referencedSymbol, targetMemberIntroduction.AssertNotNull() ),
                    //         IntermediateSymbolSemanticKind.Default ),
                    //     expression,
                    //     referenceSpecification );
                }
                else
                {
                    return new ResolvedAspectReference(
                        containingSemantic,
                        resolvedReferencedSymbol,
                        this.GetSymbolFromInjectedMember( resolvedReferencedSymbol, resolvedInjectedMember.AssertNotNull() )
                            .ToSemantic( IntermediateSymbolSemanticKind.Default ),
                        expression,
                        resolvedRootNode,
                        resolvedReferencedSymbolSourceNode,
                        targetKind,
                        isInlineable );
                }
            }
            else
            {
                return new ResolvedAspectReference(
                    containingSemantic,
                    resolvedReferencedSymbol,
                    resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Final ),
                    expression,
                    resolvedRootNode,
                    resolvedReferencedSymbolSourceNode,
                    targetKind,
                    isInlineable );
            }
        }

        private void ResolveLayerIndex(
            AspectReferenceSpecification referenceSpecification,
            MemberLayerIndex annotationLayerIndex,
            LinkerInjectedMember? targetIntroductionInjectedMember,
            MemberLayerIndex? targetIntroductionIndex,
            IReadOnlyList<(MemberLayerIndex Index, LinkerInjectedMember Override)> overrideIndices,
            out MemberLayerIndex resolvedIndex,
            out LinkerInjectedMember? resolvedInjectedMember )
        {
            resolvedInjectedMember = null;

            switch ( referenceSpecification.Order )
            {
                case AspectReferenceOrder.Original:
                    resolvedIndex = default;

                    break;

                case AspectReferenceOrder.Base:
                    // TODO: optimize.

                    var lowerOverride = overrideIndices.LastOrDefault( x => x.Index < annotationLayerIndex );

                    if ( lowerOverride.Override != null )
                    {
                        resolvedIndex = lowerOverride.Index;
                        resolvedInjectedMember = lowerOverride.Override;
                    }
                    else if ( targetIntroductionIndex != null && targetIntroductionIndex.Value < annotationLayerIndex )
                    {
                        resolvedIndex = targetIntroductionIndex.Value;
                        resolvedInjectedMember = targetIntroductionInjectedMember;
                    }
                    else
                    {
                        resolvedIndex = default;
                    }

                    break;

                case AspectReferenceOrder.Self:
                    // TODO: optimize.

                    var lowerOrEqualOverride = overrideIndices.LastOrDefault( x => x.Index <= annotationLayerIndex );

                    if ( lowerOrEqualOverride.Override != null )
                    {
                        resolvedIndex = lowerOrEqualOverride.Index;
                        resolvedInjectedMember = lowerOrEqualOverride.Override;
                    }
                    else if ( targetIntroductionIndex != null && targetIntroductionIndex.Value <= annotationLayerIndex )
                    {
                        resolvedIndex = targetIntroductionIndex.Value;
                        resolvedInjectedMember = targetIntroductionInjectedMember;
                    }
                    else
                    {
                        resolvedIndex = default;
                    }

                    break;

                case AspectReferenceOrder.Final:
                    resolvedIndex = new MemberLayerIndex( this._orderedLayers.Count, 0 );

                    break;

                default:
                    throw new AssertionFailedException();
            }
        }

        private IReadOnlyList<(MemberLayerIndex Index, LinkerInjectedMember Override)> GetOverrideIndices( ISymbol referencedSymbol )
        {
            var referencedDeclarationOverrides = this._injectionRegistry.GetOverridesForSymbol( referencedSymbol );

            // Compute indices of overrides of the referenced declaration.
            return (from overrideInjectedMember in referencedDeclarationOverrides
                    group overrideInjectedMember by overrideInjectedMember.AspectLayerId
                    into g
                    select g.Select( ( o, i ) => (Index: new MemberLayerIndex( this._layerIndex[o.AspectLayerId], i + 1 ), Override: o) )
                ).SelectMany( g => g )
                .ToReadOnlyList();
        }

        private MemberLayerIndex? GetIntroductionLogicalIndex( LinkerInjectedMember? injectedMember )
        {
            // This supports only field promotions.
            if ( injectedMember == null )
            {
                return null;
            }

            if ( injectedMember.Transformation is IReplaceMemberTransformation { ReplacedMember: { } replacedMemberRef } )
            {
                var replacedMember = replacedMemberRef.GetTarget(
                    this._finalCompilationModel,
                    ReferenceResolutionOptions.DoNotFollowRedirections );

                IDeclaration canonicalReplacedMember = replacedMember switch
                {
                    BuiltDeclaration builtDeclaration => builtDeclaration.Builder,
                    _ => replacedMember
                };

                if ( canonicalReplacedMember is IDeclarationBuilderImpl replacedBuilder )
                {
                    // This is introduced field, which is then promoted. Semantics of the field and of the property are the same.
                    return new MemberLayerIndex( this._layerIndex[replacedBuilder.ParentAdvice.AspectLayerId], 0 );
                }
                else
                {
                    // This is promoted source declaration we treat it as being present from the beginning.
                    return new MemberLayerIndex( 0, 0 );
                }
            }

            return new MemberLayerIndex( this._layerIndex[injectedMember.AspectLayerId], 0 );
        }

        private MemberLayerIndex GetAnnotationLayerIndex(
            ISymbol containingSymbol,
            ISymbol referencedSymbol,
            AspectReferenceSpecification referenceSpecification )
        {
            var referencedDeclarationOverrides = this._injectionRegistry.GetOverridesForSymbol( referencedSymbol );

            var containedInTargetOverride =
                this._injectionRegistry.IsOverrideTarget( referencedSymbol )
                && referencedDeclarationOverrides.Any(
                    x => SymbolEqualityComparer.Default.Equals(
                        this._injectionRegistry.GetSymbolForInjectedMember( x ),
                        GetPrimarySymbol( containingSymbol ) ) );

            // TODO: Optimize (most of this can be precomputed).
            // TODO: Support multiple overrides in the same layer (the memberIndex has to be determined).
            // Determine the layer from which this reference originates.
            //  * If the reference is coming from and override of the same declaration it's referencing, we need to select the correct override index.
            //  * Otherwise, treat the reference as coming from the last override of the declaration.
            var annotationLayerIndex =
                containedInTargetOverride
                    ? new MemberLayerIndex(
                        this._layerIndex[referenceSpecification.AspectLayerId],
                        referencedDeclarationOverrides
                            .Where( x => x.AspectLayerId == referenceSpecification.AspectLayerId )
                            .Select( ( x, i ) => (Symbol: x, Index: i + 1) )
                            .Single(
                                x =>
                                    SymbolEqualityComparer.Default.Equals(
                                        this._injectionRegistry.GetSymbolForInjectedMember( x.Symbol ),
                                        GetPrimarySymbol( containingSymbol ) ) )
                            .Index )
                    : new MemberLayerIndex(
                        this._layerIndex[referenceSpecification.AspectLayerId],
                        referencedDeclarationOverrides.Count( x => x.AspectLayerId == referenceSpecification.AspectLayerId ) );

            return annotationLayerIndex;
        }

        /// <summary>
        /// Resolves target symbol of the reference.
        /// </summary>
        /// <param name="containingSymbol">Symbol contains the reference.</param>
        /// <param name="referencedSymbol">Symbol that is referenced.</param>
        /// <param name="expression">Annotated expression.</param>
        /// <param name="semanticModel">Semantic model.</param>
        /// <param name="rootNode">Root of the reference that need to be rewritten (usually equal to the annotated expression).</param>
        /// <param name="targetSymbol">Symbol that the reference targets (the target symbol of the reference).</param>
        /// <param name="targetSymbolSource">Expression that identifies the target symbol (usually equal to the annotated expression).</param>
        private static void ResolveTarget(
            ISymbol containingSymbol,
            ISymbol referencedSymbol,
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            out ExpressionSyntax rootNode,
            out ISymbol targetSymbol,
            out ExpressionSyntax targetSymbolSource )
        {
            // Check whether we are referencing explicit interface implementation.
            if ( (!SymbolEqualityComparer.Default.Equals( containingSymbol.ContainingType, referencedSymbol.ContainingType )
                  && referencedSymbol.ContainingType.TypeKind == TypeKind.Interface)
                 || referencedSymbol.IsInterfaceMemberImplementation() )
            {
                // TODO: For some reason we get here in two ways (see the condition):
                //          1) The symbol is directly interface symbol (first condition matches).
                //          2) sometimes it is a "reference", i.e. contained within the current type (second condition matches).
                //       This may depend on the declaring assembly or on presence of compilation errors.

                // It's not possible to reference the introduced explicit interface implementation directly, so the reference expression
                // is in form "((<interface_type>)this).<member>", which means that the symbol points to interface member. We will transition
                // to the real member (explicit implementation) of the type before doing the rest of resolution.

                // Replace the referenced symbol with the overridden interface implementation.    
                rootNode = expression;
                targetSymbol = containingSymbol.ContainingType.AssertNotNull().FindImplementationForInterfaceMember( referencedSymbol ).AssertNotNull();
                targetSymbolSource = expression;

                return;
            }

            if ( referencedSymbol is IMethodSymbol { ContainingType: { Name: LinkerAspectReferenceSyntaxProvider.HelperTypeName } } helperMethod )
            {
                switch ( helperMethod )
                {
                    case { Name: LinkerAspectReferenceSyntaxProvider.FinalizeMemberName }:
                        // Referencing type's finalizer.
                        rootNode = expression;

                        targetSymbol = containingSymbol.ContainingType.GetMembers( "Finalize" )
                            .OfType<IMethodSymbol>()
                            .Single( m => m.Parameters.Length == 0 && m.TypeParameters.Length == 0 );

                        targetSymbolSource = expression;

                        return;

                    case { Name: LinkerAspectReferenceSyntaxProvider.PropertyMemberName }:
                        // Referencing a property.
                        switch ( expression.Parent )
                        {
                            case InvocationExpressionSyntax { ArgumentList: { Arguments: { } arguments } } invocationExpression
                                when arguments.Count == 1 && arguments[0].Expression is MemberAccessExpressionSyntax memberAccess:

                                rootNode = invocationExpression;
                                targetSymbol = semanticModel.GetSymbolInfo( memberAccess ).Symbol.AssertNotNull();
                                targetSymbolSource = memberAccess;

                                return;

                            default:
                                throw new AssertionFailedException();
                        }

                    case { } when SymbolHelpers.GetOperatorKindFromName( helperMethod.Name ) is not OperatorKind.None and var operatorKind:
                        // Referencing an operator.
                        if ( operatorKind.GetCategory() == OperatorCategory.Binary )
                        {
                            rootNode = expression;

                            targetSymbol = containingSymbol.ContainingType.GetMembers( referencedSymbol.Name )
                                .OfType<IMethodSymbol>()
                                .Single(
                                    m =>
                                        m.Parameters.Length == 2
                                        && SignatureTypeSymbolComparer.Instance.Equals( m.Parameters[0].Type, helperMethod.Parameters[0].Type )
                                        && SignatureTypeSymbolComparer.Instance.Equals( m.Parameters[1].Type, helperMethod.Parameters[1].Type )
                                        && SignatureTypeSymbolComparer.Instance.Equals( m.ReturnType, helperMethod.ReturnType ) );

                            targetSymbolSource = expression;

                            return;
                        }
                        else
                        {
                            rootNode = expression;

                            targetSymbol = containingSymbol.ContainingType.GetMembers( referencedSymbol.Name )
                                .OfType<IMethodSymbol>()
                                .Single(
                                    m =>
                                        m.Parameters.Length == 1
                                        && SignatureTypeSymbolComparer.Instance.Equals( m.Parameters[0].Type, helperMethod.Parameters[0].Type )
                                        && SignatureTypeSymbolComparer.Instance.Equals( m.ReturnType, helperMethod.ReturnType ) );

                            targetSymbolSource = expression;

                            return;
                        }

                    default:
                        throw new AssertionFailedException();
                }
            }

            rootNode = expression;
            targetSymbol = referencedSymbol;
            targetSymbolSource = expression;
        }

        private static AspectReferenceTargetKind ResolveExpressionTarget( ISymbol referencedSymbol, ExpressionSyntax expression )
        {
            switch (referencedSymbol, expression)
            {
                case (IPropertySymbol, { Parent: AssignmentExpressionSyntax }):
                    return AspectReferenceTargetKind.PropertySetAccessor;

                case (IPropertySymbol, _):
                    return AspectReferenceTargetKind.PropertyGetAccessor;

                case (IFieldSymbol, { Parent: AssignmentExpressionSyntax }):
                    return AspectReferenceTargetKind.PropertySetAccessor;

                case (IFieldSymbol, _):
                    return AspectReferenceTargetKind.PropertyGetAccessor;

                case (IEventSymbol, { Parent: AssignmentExpressionSyntax { OperatorToken: { RawKind: (int) SyntaxKind.AddAssignmentExpression } } }):
                    return AspectReferenceTargetKind.EventAddAccessor;

                case (IEventSymbol, { Parent: AssignmentExpressionSyntax { OperatorToken: { RawKind: (int) SyntaxKind.SubtractAssignmentExpression } } }):
                    return AspectReferenceTargetKind.EventRemoveAccessor;

                case (IEventSymbol, _):
                    return AspectReferenceTargetKind.EventRaiseAccessor;

                default:
                    throw new AssertionFailedException();
            }
        }

        private static bool HasImplicitImplementation( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IPropertySymbol property when property.IsAutoProperty().GetValueOrDefault():
                case IEventSymbol @event when @event.IsExplicitInterfaceEventField() || @event.IsEventField():
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Translates the resolved injected member to the same kind of symbol as the referenced symbol.
        /// </summary>
        /// <param name="referencedSymbol"></param>
        /// <param name="resolvedInjectedMember"></param>
        /// <returns></returns>
        private ISymbol GetSymbolFromInjectedMember( ISymbol referencedSymbol, LinkerInjectedMember resolvedInjectedMember )
        {
            var symbol = this._injectionRegistry.GetSymbolForInjectedMember( resolvedInjectedMember );

            return GetCorrespondingSymbolForResolvedSymbol( referencedSymbol, symbol );
        }

        private static ISymbol? GetOverriddenSymbol( ISymbol symbol )
            => symbol switch
            {
                IMethodSymbol methodSymbol => methodSymbol.OverriddenMethod,
                IPropertySymbol propertySymbol => propertySymbol.OverriddenProperty,
                IEventSymbol eventSymbol => eventSymbol.OverriddenEvent,
                _ => throw new AssertionFailedException()
            };

        private static ISymbol? GetPrimarySymbol( ISymbol symbol )
            => symbol switch
            {
                IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } methodSymbol => methodSymbol.AssociatedSymbol,
                IMethodSymbol { MethodKind: MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise } methodSymbol => methodSymbol
                    .AssociatedSymbol,
                IMethodSymbol methodSymbol => methodSymbol,
                IPropertySymbol propertySymbol => propertySymbol,
                IEventSymbol eventSymbol => eventSymbol,
                _ => throw new AssertionFailedException()
            };

        /// <summary>
        /// Gets a symbol the "new" symbol is hiding.
        /// </summary>
        /// <param name="symbol">The hiding symbol.</param>
        /// <param name="hiddenSymbol">The hidden symbol.</param>
        /// <returns>Hidden symbol or null.</returns>
        private bool TryGetHiddenSymbol( ISymbol symbol, [NotNullWhen( true )] out ISymbol? hiddenSymbol )
        {
            var currentType = symbol.ContainingType.BaseType;

            while ( currentType != null )
            {
                var matchingSymbol = currentType.GetMembers()
                    .SingleOrDefault(
                        member => member.IsVisibleTo( this._intermediateCompilation, symbol )
                                  && SignatureTypeSymbolComparer.Instance.Equals( symbol, member ) );

                if ( matchingSymbol != null )
                {
                    hiddenSymbol = matchingSymbol;

                    return true;
                }

                currentType = currentType.BaseType;
            }

            hiddenSymbol = null;

            return false;
        }

        /// <summary>
        /// Gets a symbol that corresponds to the referenced symbol for the resolved symbol. 
        /// This has a meaning when referenced symbol was a property/event accessor and the resolved symbol is the property/event itself.
        /// </summary>
        /// <param name="referencedSymbol"></param>
        /// <param name="resolvedSymbol"></param>
        /// <returns></returns>
        private static ISymbol GetCorrespondingSymbolForResolvedSymbol( ISymbol referencedSymbol, ISymbol resolvedSymbol )
        {
            switch (referencedSymbol, resolvedSymbol)
            {
                case (IMethodSymbol { MethodKind: MethodKind.Ordinary }, IMethodSymbol { MethodKind: MethodKind.Ordinary }):
                case (IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation }, IMethodSymbol { MethodKind: MethodKind.Ordinary }):
                case (IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation },
                    IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation }):
                case (IMethodSymbol { MethodKind: MethodKind.Destructor }, IMethodSymbol { MethodKind: MethodKind.Ordinary }):
                case (IMethodSymbol { MethodKind: MethodKind.Conversion or MethodKind.UserDefinedOperator }, IMethodSymbol { MethodKind: MethodKind.Ordinary }):
                case (IPropertySymbol, IPropertySymbol):
                case (IEventSymbol, IEventSymbol):
                case (IFieldSymbol, IFieldSymbol):
                    return resolvedSymbol;

                case (IMethodSymbol { MethodKind: MethodKind.PropertyGet }, IPropertySymbol):
                    // This seems to happen only in invalid compilations.
                    throw new AssertionFailedException( Justifications.CoverageMissing );

                // return propertySymbol.GetMethod.AssertNotNull();

                case (IMethodSymbol { MethodKind: MethodKind.PropertySet }, IPropertySymbol):
                    // This seems to happen only in invalid compilations.
                    throw new AssertionFailedException( Justifications.CoverageMissing );

                // return propertySymbol.SetMethod.AssertNotNull();

                case (IMethodSymbol { MethodKind: MethodKind.EventAdd }, IEventSymbol):
                    // This seems to happen only in invalid compilations.
                    throw new AssertionFailedException( Justifications.CoverageMissing );

                // return eventSymbol.AddMethod.AssertNotNull();

                case (IMethodSymbol { MethodKind: MethodKind.EventRemove }, IEventSymbol):
                    // This seems to happen only in invalid compilations.
                    throw new AssertionFailedException( Justifications.CoverageMissing );

                // return eventSymbol.RemoveMethod.AssertNotNull();

                default:
                    throw new AssertionFailedException();
            }
        }

        private readonly struct MemberLayerIndex : IComparable<MemberLayerIndex>, IEquatable<MemberLayerIndex>
        {
            public int LayerIndex { get; }

            public int MemberIndex { get; }

            public MemberLayerIndex( int layerIndex, int memberIndex )
            {
                this.LayerIndex = layerIndex;
                this.MemberIndex = memberIndex;
            }

            public int CompareTo( MemberLayerIndex other )
            {
                var layerDiff = this.LayerIndex - other.LayerIndex;

                if ( layerDiff == 0 )
                {
                    return this.MemberIndex - other.MemberIndex;
                }
                else
                {
                    return layerDiff;
                }
            }

            public bool Equals( MemberLayerIndex other )
            {
                return this.CompareTo( other ) == 0;
            }

            public override bool Equals( object? obj )
            {
                return obj is MemberLayerIndex mli && this.Equals( mli );
            }

            public override int GetHashCode()
            {
                return HashCode.Combine( this.LayerIndex, this.MemberIndex );
            }

            public override string ToString()
            {
                return $"({this.LayerIndex}, {this.MemberIndex})";
            }

            public static bool operator ==( MemberLayerIndex a, MemberLayerIndex b ) => a.Equals( b );

            public static bool operator !=( MemberLayerIndex a, MemberLayerIndex b ) => !a.Equals( b );

            public static bool operator <( MemberLayerIndex a, MemberLayerIndex b ) => a.CompareTo( b ) < 0;

            public static bool operator <=( MemberLayerIndex a, MemberLayerIndex b ) => a.CompareTo( b ) <= 0;

            public static bool operator >( MemberLayerIndex a, MemberLayerIndex b ) => a.CompareTo( b ) > 0;

            public static bool operator >=( MemberLayerIndex a, MemberLayerIndex b ) => a.CompareTo( b ) >= 0;
        }
    }
}