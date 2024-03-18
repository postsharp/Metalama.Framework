﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using System;
using System.Collections.Concurrent;
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
//  * Override z-1-1
//  ...
//  * Override z-k-l (there are multiple overrides of the same declaration in one layer and multiple aspect instances).
//  ...
//  * Override n
//  * Target declaration,final semantic)

// Each of above correspond to an aspect layer in the global order.
// The reference we are trying to resolve also originates in of the aspect layers.

// Declaration semantics projected to global aspect layer order:
// * Layer (0, 0, 0):   Overridden declaration (base class declaration).
// * Layer (0, 0, 0):   Target declaration, default semantic (if from source code).
// * Layer (0, 0, 0):   Target declaration, base semantic (if introduced, no overridden declaration).
// ...
// * Layer (k, 0):   Target declaration, default semantic (if introduced).
// * Layer (k, 1, 1):   After introduction 1-1.
// * Layer (k, 1, 2):   After override 1-1 (same layer as introduction).
// ...
// * Layer (l_1, 1, 1): After override 2-1 (layer with multiple overrides).
// ...
// * Layer (l_1, 1, q_1): After override 2-q_1 in aspect instance 1.
// ...
// * Layer (l_n, p, q_n): After override q_n in aspect instance p.
// ...
// * Layer (m, 0):   Target declaration, final semantic.

// AspectReferenceOrder resolution:
//  * Base - resolved to the last override preceding the referencing layer.
//  * Previous - resolved to the last preceding override.
//  * Current - resolved to the last override preceding or equal to the referencing layer.
//  * Final - resolved to the last override in the order or final semantic.

// Special cases:
//  * Promoted fields do not count as introductions. The layer of the promotion target applies.
//    Source promoted fields are treated as source declarations. Introduced and then promoted fields
//    are treated as being introduced at the point of field introduction.

// Notes:
//  * Base and Self are different only for layers that override the referenced declaration.

namespace Metalama.Framework.Engine.Linking;

/// <summary>
/// Resolves aspect references.
/// </summary>
internal sealed class AspectReferenceResolver
{
    private readonly LinkerInjectionRegistry _injectionRegistry;
    private readonly IReadOnlyList<AspectLayerId> _orderedLayers;
    private readonly IReadOnlyDictionary<AspectLayerId, int> _layerIndex;
    private readonly CompilationModel _finalCompilationModel;
    private readonly SafeSymbolComparer _comparer;
    private readonly ConcurrentDictionary<ISymbol, IReadOnlyList<OverrideIndex>> _overrideIndicesCache;
    private readonly PartialCompilation _intermediateCompilation;

    public AspectReferenceResolver(
        PartialCompilation intermediateCompilation,
        LinkerInjectionRegistry injectionRegistry,
        IReadOnlyList<OrderedAspectLayer> orderedAspectLayers,
        CompilationModel finalCompilationModel,
        CompilationContext intermediateCompilationContext )
    {
        this._intermediateCompilation = intermediateCompilation;
        this._injectionRegistry = injectionRegistry;

        var indexedLayers =
            new[] { AspectLayerId.Null }.Concat( orderedAspectLayers.SelectAsReadOnlyList( x => x.AspectLayerId ) )
                .Select( ( al, i ) => (AspectLayerId: al, Index: i) )
                .ToReadOnlyList();

        this._orderedLayers = indexedLayers.SelectAsImmutableArray( x => x.AspectLayerId );
        this._layerIndex = indexedLayers.ToDictionary( x => x.AspectLayerId, x => x.Index );
        this._finalCompilationModel = finalCompilationModel;
        this._comparer = intermediateCompilationContext.SymbolComparer;
        this._overrideIndicesCache = new ConcurrentDictionary<ISymbol, IReadOnlyList<OverrideIndex>>( intermediateCompilationContext.SymbolComparer );
    }

    public ResolvedAspectReference? Resolve(
        IntermediateSymbolSemantic<IMethodSymbol> containingSemantic,
        IMethodSymbol? containingLocalFunction,
        ExpressionSyntax expression,
        AspectReferenceSpecification referenceSpecification,
        SemanticModel semanticModel )
    {
        var annotationSymbol = this.GetSymbolFromDeclarationId( referenceSpecification );

        // Get the reference root node, the local symbol that is referenced, and the node that was the source for the symbol.
        //   1) Normal reference:
        //     this.Foo()
        //     ^^^^^^^^ - aspect reference (symbol points directly to the member)
        //     ^^^^^^^^ - symbol source
        //     ^^^^^^^^ - resolved root node
        //   2) Interface member references:
        //     ((IInterface)this).Foo()
        //                        ^^^ - aspect reference (symbol points to interface member)
        //                        ^^^ - symbol source
        //     ^^^^^^^^^^^^^^^^^^^^^^ - resolved root node
        //   3) Referencing a get-only property "setter":
        //     __LinkerInjectionHelpers__.__Property(this.Foo) = 42;
        //     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ - aspect reference (symbol points to the special linker helper)
        //                                           ^^^^^^^^  - symbol source
        //     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ - resolved root node
        //   4) Awaitable async-void method:
        //     __LinkerInjectionHelpers__.__AsyncVoidMethod(this.Foo)(<args>)
        //                                                  ^^^^^^^^  - aspect reference
        //                                                  ^^^^^^^^  - symbol source
        //     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ - resolved root node

        if ( !this.TryResolveTarget(
                containingSemantic.Symbol,
                annotationSymbol,
                expression,
                semanticModel,
                out var resolvedRootNode,
                out var resolvedReferencedSymbol,
                out var resolvedReferencedSymbolSourceNode ) )
        {
            return null;
        }

        // resolvedRootNode is the node that will be replaced when rewriting the aspect reference.
        // resolvedReferencedSymbol is the real target of the reference.
        // resolvedReferencedSymbolSourceNode is the node that will be rewritten when renaming the aspect reference (e.g. redirecting to a particular override).

        var targetKind = referenceSpecification.TargetKind;

        if ( targetKind == AspectReferenceTargetKind.Self && resolvedReferencedSymbol is IPropertySymbol or IEventSymbol or IFieldSymbol )
        {
            // Resolves the symbol based on expression - this is used when aspect reference targets property/event/field
            // but it is not specified whether the getter/setter/adder/remover is targeted.
            targetKind = ResolveExpressionTarget( resolvedReferencedSymbol, expression );
        }

        // At this point we should always target a method or a specific target.
        Invariant.AssertNot( resolvedReferencedSymbol is IPropertySymbol or IEventSymbol or IFieldSymbol && targetKind == AspectReferenceTargetKind.Self );

        var annotationLayerIndex = this.GetAnnotationLayerIndex( containingSemantic.Symbol );

        // If the override target was introduced, determine the index.
        var targetIntroductionInjectedMember = this._injectionRegistry.GetInjectedMemberForSymbol( resolvedReferencedSymbol );
        var targetIntroductionIndex = this.GetIntroductionLogicalIndex( targetIntroductionInjectedMember );

        var overrideIndices = this.GetOverrideIndices( resolvedReferencedSymbol );

        Invariant.Assert(
            targetIntroductionIndex == null || overrideIndices.All( o => targetIntroductionIndex < o.Index )
                                            || !HasImplicitImplementation( resolvedReferencedSymbol ) );

        this.ResolveLayerIndex(
            referenceSpecification,
            annotationLayerIndex,
            resolvedReferencedSymbol,
            targetIntroductionInjectedMember,
            targetIntroductionIndex,
            overrideIndices,
            out var resolvedIndex,
            out var resolvedInjectedMember );

        // At this point resolvedIndex should be 0, equal to target introduction index, this._orderedLayers.Count or be equal to index of one of the overrides.
        Invariant.Assert(
            resolvedIndex == default
            || resolvedIndex == new MemberLayerIndex( this._orderedLayers.Count, 0, 0 )
            || overrideIndices.Any( x => x.Index == resolvedIndex )
            || resolvedIndex == targetIntroductionIndex );

        if ( overrideIndices.Count > 0 && resolvedIndex == overrideIndices[overrideIndices.Count - 1].Index )
        {
            // If we have resolved to the last override, transition to the final declaration index.
            resolvedIndex = new MemberLayerIndex( this._orderedLayers.Count, 0, 0 );
        }

        if ( resolvedIndex == default )
        {
            // Resolved to the initial version of the symbol (before any aspects).

            if ( targetIntroductionInjectedMember == null
                 || (targetIntroductionInjectedMember.Transformation is IReplaceMemberTransformation { ReplacedMember: { } replacedMember }
                     && replacedMember.GetTarget( this._finalCompilationModel, ReferenceResolutionOptions.DoNotFollowRedirections ).GetSymbol() != null) )
            {
                // There is no introduction, i.e. this is a user source symbol (or a promoted field) => reference the version present in source.
                var declaredInCurrentType = this._comparer.Equals( containingSemantic.Symbol.ContainingType, resolvedReferencedSymbol.ContainingType );

                var targetSemantic =
                    (!declaredInCurrentType && resolvedReferencedSymbol.IsVirtual)
                    || (declaredInCurrentType && resolvedReferencedSymbol is { IsOverride: true, IsSealed: false } or { IsVirtual: true }
                                              && overrideIndices.Count == 0)
                        ? resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Base )
                        : resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default );

                return CreateResolved( targetSemantic );
            }
            else
            {
                // There is an introduction and this reference points to a state before that introduction.
                return CreateResolved( resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Base ) );
            }
        }
        else if ( targetIntroductionInjectedMember != null && resolvedIndex < targetIntroductionIndex )
        {
            // Resolved to a version before the symbol was introduced.
            // The only valid case are introduced promoted fields.
            if ( targetIntroductionInjectedMember.Transformation is IReplaceMemberTransformation { ReplacedMember: { } replacedMember }
                 && replacedMember.GetTarget( this._finalCompilationModel, ReferenceResolutionOptions.DoNotFollowRedirections ).GetSymbol() == null )
            {
                // This is the same as targeting the property.
                return CreateResolved( resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) );
            }
            else
            {
                throw new AssertionFailedException(
                    $"Resolving {resolvedReferencedSymbol} aspect reference to a non-initial state before the introduction is valid only for replaced introduced members." );
            }
        }
        else if ( targetIntroductionInjectedMember != null && resolvedIndex == targetIntroductionIndex )
        {
            // Targeting the introduced version of the symbol.
            // The only way to get here is for declarations with implicit implementation, everything else is not valid.

            if ( HasImplicitImplementation( resolvedReferencedSymbol ) )
            {
                return CreateResolved( resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default ) );
            }
            else
            {
                throw new AssertionFailedException(
                    $"Resolving {resolvedReferencedSymbol} aspect reference to the introduction is not allowed because the declaration does not have implicit body." );
            }
        }
        else if ( resolvedIndex < new MemberLayerIndex( this._orderedLayers.Count, 0, 0 ) )
        {
            // One particular override.
            return CreateResolved(
                this.GetSymbolFromInjectedMember( resolvedReferencedSymbol, resolvedInjectedMember.AssertNotNull() )
                    .ToSemantic( IntermediateSymbolSemanticKind.Default ) );
        }
        else if ( resolvedIndex == new MemberLayerIndex( this._orderedLayers.Count, 0, 0 ) )
        {
            var targetSemantic =
                overrideIndices.Count > 0
                    ? resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Final )
                    : resolvedReferencedSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default );

            // The version after all aspects.
            return CreateResolved( targetSemantic );
        }
        else
        {
            throw new AssertionFailedException( $"Resolving {resolvedReferencedSymbol} aspect reference to {resolvedIndex} is not supported." );
        }

        ResolvedAspectReference CreateResolved( IntermediateSymbolSemantic resolvedSemantic )
        {
            return new ResolvedAspectReference(
                containingSemantic,
                containingLocalFunction,
                resolvedReferencedSymbol,
                resolvedSemantic,
                expression,
                resolvedRootNode,
                resolvedReferencedSymbolSourceNode,
                targetKind,
                referenceSpecification.Flags );
        }
    }

    private ISymbol? GetSymbolFromDeclarationId( AspectReferenceSpecification referenceSpecification )
    {
        if ( referenceSpecification.TargetDeclarationId != null )
        {
            var candidateSymbols = DocumentationCommentId.GetSymbolsForDeclarationId( referenceSpecification.TargetDeclarationId.Value.Id, this._intermediateCompilation.Compilation );

            /* TODO: Open generics and possibly other declarations cannot be resolved using this method.
             *       Uncomment the following too see failing tests.
             * Invariant.Assert( candidateSymbols.Length == 1 );
             */

            if ( candidateSymbols.Length == 1 )
            {
                return candidateSymbols[0];
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }

    private static ISymbol? GetSymbolFromSemanticModel( SemanticModel semanticModel, SyntaxNode node )
    {
        var nodeWithSymbol = node switch
        {
            ConditionalAccessExpressionSyntax conditionalAccess => GetConditionalMemberName( conditionalAccess ),
            _ => node
        };

        var symbolInfo = semanticModel.GetSymbolInfo( nodeWithSymbol );
        var referencedSymbol = symbolInfo.Symbol;

        if ( referencedSymbol == null )
        {
            // This is a workaround for a problem I don't fully understand.
            // We can get here at design time, in the preview pipeline. In this case, generating exactly correct code
            // is not important. At compile time, an invalid symbol would result in a failed compilation, which is ok.

            if ( symbolInfo.CandidateSymbols.Length == 1 )
            {
                return symbolInfo.CandidateSymbols[0];
            }
            else
            {
                return null;
            }
        }

        return referencedSymbol;

        static MemberBindingExpressionSyntax GetConditionalMemberName( ConditionalAccessExpressionSyntax conditionalAccess )
        {
            var walker = new ConditionalAccessExpressionWalker();

            walker.Visit( conditionalAccess );

            return walker.FoundName.AssertNotNull();
        }
    }

    private void ResolveLayerIndex(
        AspectReferenceSpecification referenceSpecification,
        MemberLayerIndex annotationLayerIndex,
        ISymbol referencedSymbol,
        InjectedMember? targetIntroductionInjectedMember,
        MemberLayerIndex? targetIntroductionIndex,
        IReadOnlyList<OverrideIndex> overrideIndices,
        out MemberLayerIndex resolvedIndex,
        out InjectedMember? resolvedInjectedMember )
    {
        resolvedInjectedMember = null;

        switch ( referenceSpecification.Order )
        {
            case AspectReferenceOrder.Base:
                // TODO: optimize.

                var lowerOverride = overrideIndices.LastOrDefault( x => x.Index.LayerIndex < annotationLayerIndex.LayerIndex );

                if ( lowerOverride.Override != null )
                {
                    resolvedIndex = lowerOverride.Index;
                    resolvedInjectedMember = lowerOverride.Override;
                }
                else if ( targetIntroductionIndex != null && targetIntroductionIndex.Value < annotationLayerIndex
                                                          && HasImplicitImplementation( referencedSymbol ) )
                {
                    resolvedIndex = targetIntroductionIndex.Value;
                    resolvedInjectedMember = targetIntroductionInjectedMember;
                }
                else
                {
                    resolvedIndex = default;
                }

                break;

            case AspectReferenceOrder.Previous:

                var previousOverride = overrideIndices.LastOrDefault( x => x.Index < annotationLayerIndex );

                if ( previousOverride.Override != null )
                {
                    resolvedIndex = previousOverride.Index;
                    resolvedInjectedMember = previousOverride.Override;
                }
                else if ( targetIntroductionIndex != null && targetIntroductionIndex.Value < annotationLayerIndex
                                                          && HasImplicitImplementation( referencedSymbol ) )
                {
                    resolvedIndex = targetIntroductionIndex.Value;
                    resolvedInjectedMember = targetIntroductionInjectedMember;
                }
                else
                {
                    resolvedIndex = default;
                }

                break;

            case AspectReferenceOrder.Current:
                // TODO: optimize.

                var lowerOrEqualOverride = overrideIndices.LastOrDefault( x => x.Index.LayerIndex <= annotationLayerIndex.LayerIndex );

                if ( lowerOrEqualOverride.Override != null )
                {
                    resolvedIndex = lowerOrEqualOverride.Index;
                    resolvedInjectedMember = lowerOrEqualOverride.Override;
                }
                else if ( targetIntroductionIndex != null && targetIntroductionIndex.Value <= annotationLayerIndex )
                {
                    Invariant.Assert( HasImplicitImplementation( referencedSymbol ) );

                    resolvedIndex = targetIntroductionIndex.Value;
                    resolvedInjectedMember = targetIntroductionInjectedMember;
                }
                else
                {
                    resolvedIndex = default;
                }

                break;

            case AspectReferenceOrder.Final:
                resolvedIndex = new MemberLayerIndex( this._orderedLayers.Count, 0, 0 );

                break;

            default:
                throw new AssertionFailedException( $"Unexpected value for AspectReferenceOrder: {referenceSpecification.Order}." );
        }
    }

    private IReadOnlyList<OverrideIndex> GetOverrideIndices( ISymbol referencedSymbol )
    {
        // PERF: Caching prevents reallocation for every override.
        return this._overrideIndicesCache.GetOrAdd( referencedSymbol, Get, this );

        static IReadOnlyList<OverrideIndex> Get( ISymbol referencedSymbol, AspectReferenceResolver @this )
        {
            var referencedDeclarationOverrides = @this._injectionRegistry.GetOverridesForSymbol( referencedSymbol );

            // Order coming from transformation needs to be incremented by 1, because 0 represents state before the aspect layer.
            return
                referencedDeclarationOverrides
                    .SelectAsReadOnlyList(
                        x =>
                        {
                            var injectedMember = @this._injectionRegistry.GetInjectedMemberForSymbol( x ).AssertNotNull();

                            return
                                new OverrideIndex(
                                    @this.GetMemberLayerIndex( injectedMember ),
                                    injectedMember );
                        } )
                    .Materialize()
                    .AssertSorted( x => x.Index );
        }
    }

    private MemberLayerIndex? GetIntroductionLogicalIndex( InjectedMember? injectedMember )
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
                var fieldInjectionTransformation =
                    this._injectionRegistry.GetTransformationForBuilder( replacedBuilder )
                    ?? throw new AssertionFailedException( $"Could not find transformation for {replacedBuilder}" );

                // Order coming from transformation needs to be incremented by 1, because 0 represents state before the aspect layer.
                return
                    new MemberLayerIndex(
                        this._layerIndex[replacedBuilder.ParentAdvice.AspectLayerId],
                        fieldInjectionTransformation.OrderWithinPipelineStepAndType + 1,
                        fieldInjectionTransformation.OrderWithinPipelineStepAndTypeAndAspectInstance + 1 );
            }
            else
            {
                // This is promoted source declaration we treat it as being present from the beginning.
                return new MemberLayerIndex( 0, 0, 0 );
            }
        }

        return this.GetMemberLayerIndex( injectedMember );
    }

    private MemberLayerIndex GetAnnotationLayerIndex( ISymbol containingSymbol )
    {
        var containingInjectedMember =
            this._injectionRegistry.GetInjectedMemberForSymbol( containingSymbol )
            ?? throw new AssertionFailedException( $"Could not find injected member for {containingSymbol}." );

        return this.GetMemberLayerIndex( containingInjectedMember );
    }

    private MemberLayerIndex GetMemberLayerIndex( InjectedMember injectedMember )
        => injectedMember.Transformation != null
            ? new MemberLayerIndex(
                this._layerIndex[injectedMember.AspectLayerId.AssertNotNull()],
                injectedMember.Transformation.OrderWithinPipelineStepAndType + 1,
                injectedMember.Transformation.OrderWithinPipelineStepAndTypeAndAspectInstance + 1 )
            : new MemberLayerIndex( 0, 0, 0 );

    /// <summary>
    /// Resolves target symbol of the reference.
    /// </summary>
    /// <param name="containingSymbol">Symbol contains the reference.</param>
    /// <param name="annotationSymbol">Symbol that is was referenced in the annotation.</param>
    /// <param name="expression">Annotated expression.</param>
    /// <param name="semanticModel">Semantic model.</param>
    /// <param name="rootNode">Root of the reference that need to be rewritten (usually equal to the annotated expression).</param>
    /// <param name="targetSymbol">Symbol that the reference targets (the target symbol of the reference).</param>
    /// <param name="targetSymbolSource">Expression that identifies the target symbol (usually equal to the annotated expression).</param>
    private bool TryResolveTarget(
        ISymbol containingSymbol,
        ISymbol? annotationSymbol,
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        [NotNullWhen( true )] out ExpressionSyntax? rootNode,
        [NotNullWhen( true )] out ISymbol? targetSymbol,
        [NotNullWhen( true )] out ExpressionSyntax? targetSymbolSource )
    {
        // TODO: I think this should be removed.
        // Check whether we are referencing explicit interface implementation.
        if ( annotationSymbol == null )
        {
            var referencedSymbol = GetSymbolFromSemanticModel( semanticModel, expression ).AssertNotNull();

            if ( (!this._comparer.Equals( containingSymbol.ContainingType, referencedSymbol.ContainingType )
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

                return true;
            }
        }

        if ( expression is { Parent.Parent.Parent.Parent: InvocationExpressionSyntax { Expression: { } wrappingExpression } }
             && wrappingExpression is InvocationExpressionSyntax
             {
                Expression: MemberAccessExpressionSyntax
                 {
                     Expression: IdentifierNameSyntax { Identifier.ValueText: LinkerInjectionHelperProvider.HelperTypeName },
                     Name: NameSyntax { } wrappingHelperName
                 }
             } )
        {
            // Wrapping helper methods are used in special cases where the generated expression needs to be additionally wrapped.
            var helperMethodName =
                wrappingHelperName switch
                {
                    IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                    _ => throw new AssertionFailedException( $"Unknown name: {wrappingHelperName}" ),
                };

            switch ( helperMethodName )
            {
                case LinkerInjectionHelperProvider.AsyncVoidMethodMemberName:
                    // Referencing async-void method.
                    rootNode = wrappingExpression;
                    targetSymbolSource = expression;
                    targetSymbol = GetSymbolFromSemanticModel( semanticModel, expression ).AssertNotNull();

                    return true;

                default:
                    throw new AssertionFailedException( $"Unexpected wrapping helper method: '{helperMethodName}'." );
            }
        }

        if ( expression is MemberAccessExpressionSyntax { 
            Expression: IdentifierNameSyntax { Identifier.ValueText: LinkerInjectionHelperProvider.HelperTypeName },
            Name: { } name } )
        {
            var helperMethodName =
                name switch
                {
                    IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
                    GenericNameSyntax genericName => genericName.Identifier.ValueText,
                    _ => throw new AssertionFailedException( $"Unknown name: {name}" ),
                };

            switch ( helperMethodName )
            {
                case LinkerInjectionHelperProvider.FinalizeMemberName:
                    // Referencing type's finalizer.
                    rootNode = expression;
                    targetSymbolSource = expression;

                    targetSymbol = containingSymbol.ContainingType.GetMembers( "Finalize" )
                        .OfType<IMethodSymbol>()
                        .Single( m => m.Parameters.Length == 0 && m.TypeParameters.Length == 0 );

                    return true;

                case LinkerInjectionHelperProvider.StaticConstructorMemberName:
                    // Referencing type's constructor.
                    switch ( expression.Parent )
                    {
                        case InvocationExpressionSyntax { ArgumentList.Arguments: [] }:
                            rootNode = expression;
                            targetSymbol = containingSymbol.ContainingType.StaticConstructors.FirstOrDefault().AssertNotNull();
                            targetSymbolSource = expression;

                            return true;

                        default:
                            throw new AssertionFailedException( $"Unexpected static constructor expression: '{expression.Parent}'." );
                    }

                case LinkerInjectionHelperProvider.ConstructorMemberName:
                    // Referencing type's constructor.
                    switch ( expression.Parent )
                    {
                        case InvocationExpressionSyntax
                        {
                            ArgumentList.Arguments: [{ Expression: ObjectCreationExpressionSyntax objectCreation }]
                        } invocationExpression:

                            rootNode = invocationExpression;

                            // TODO: This is hacky - since we don't see any introduced parameter while expanding a template, the target symbol of the aspect
                            //       reference is not valid (either unresolved or pointing to a wrong constructor).
                            //       Using the override target (which is correctly resolved) is a temporary solution until we need to have constructor invokers.

                            var overrideTarget =
                                this._injectionRegistry.GetOverrideTarget( containingSymbol )
                                ?? throw new AssertionFailedException( $"Could not resolve override target for '{containingSymbol}'" );

                            targetSymbol = overrideTarget;
                            targetSymbolSource = objectCreation;

                            return true;

                        default:
                            throw new AssertionFailedException( $"Unexpected constructor expression: '{expression.Parent}'." );
                    }

                case LinkerInjectionHelperProvider.PropertyMemberName:
                    // Referencing a property.
                    switch ( expression.Parent )
                    {
                        case InvocationExpressionSyntax
                        {
                            ArgumentList.Arguments: [{ Expression: MemberAccessExpressionSyntax memberAccess }]
                        } invocationExpression:

                            rootNode = invocationExpression;
                            targetSymbol = 
                                annotationSymbol
                                ?? semanticModel.GetSymbolInfo( memberAccess ).Symbol.AssertNotNull();
                            targetSymbolSource = memberAccess;

                            return true;

                        default:
                            throw new AssertionFailedException( $"Unexpected property expression: '{expression.Parent}'." );
                    }

                case not null when SymbolHelpers.GetOperatorKindFromName( helperMethodName ) is not OperatorKind.None and var operatorKind:
                    // Referencing an operator.
                    rootNode = expression;
                    targetSymbolSource = expression;

                    if ( annotationSymbol == null )
                    {
                        var referencedSymbol = GetSymbolFromSemanticModel( semanticModel, expression ).AssertNotNull();
                        var helperMethod = (IMethodSymbol) referencedSymbol;

                        if ( operatorKind.GetCategory() == OperatorCategory.Binary )
                        {
                            targetSymbol =
                                containingSymbol.ContainingType.GetMembers( referencedSymbol.Name )
                                .OfType<IMethodSymbol>()
                                .Single(
                                    m =>
                                        m.Parameters.Length == 2
                                        && SignatureTypeSymbolComparer.Instance.Equals( m.Parameters[0].Type, helperMethod.Parameters[0].Type )
                                        && SignatureTypeSymbolComparer.Instance.Equals( m.Parameters[1].Type, helperMethod.Parameters[1].Type )
                                        && SignatureTypeSymbolComparer.Instance.Equals( m.ReturnType, helperMethod.ReturnType ) );
                        }
                        else
                        {
                            targetSymbol = containingSymbol.ContainingType.GetMembers( referencedSymbol.Name )
                                .OfType<IMethodSymbol>()
                                .Single(
                                    m =>
                                        m.Parameters.Length == 1
                                        && SignatureTypeSymbolComparer.Instance.Equals( m.Parameters[0].Type, helperMethod.Parameters[0].Type )
                                        && SignatureTypeSymbolComparer.Instance.Equals( m.ReturnType, helperMethod.ReturnType ) );
                        }
                    }
                    else
                    {
                        targetSymbol = annotationSymbol;
                    }

                    return true;

                default:
                    throw new AssertionFailedException( $"Unexpected helper method: '{helperMethodName}'." );
            }
        }

        rootNode = expression;
        targetSymbol = annotationSymbol ?? GetSymbolFromSemanticModel( semanticModel, expression ).AssertNotNull();
        targetSymbolSource = expression;

        return true;
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

            case (IEventSymbol, { Parent: AssignmentExpressionSyntax { OperatorToken.RawKind: (int) SyntaxKind.AddAssignmentExpression } }):
                return AspectReferenceTargetKind.EventAddAccessor;

            case (IEventSymbol, { Parent: AssignmentExpressionSyntax { OperatorToken.RawKind: (int) SyntaxKind.SubtractAssignmentExpression } }):
                return AspectReferenceTargetKind.EventRemoveAccessor;

            case (IEventSymbol, _):
                return AspectReferenceTargetKind.EventRaiseAccessor;

            default:
                throw new AssertionFailedException( $"Unexpected referenced symbol: '{referencedSymbol}'" );
        }
    }

    private static bool HasImplicitImplementation( ISymbol symbol )
    {
        switch ( symbol )
        {
            case IFieldSymbol:
            case IPropertySymbol property when property.IsAutoProperty().GetValueOrDefault():
            case IEventSymbol @event when @event.IsExplicitInterfaceEventField() || @event.IsEventField().GetValueOrDefault():
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
    private ISymbol GetSymbolFromInjectedMember( ISymbol referencedSymbol, InjectedMember resolvedInjectedMember )
    {
        var symbol = this._injectionRegistry.GetSymbolForInjectedMember( resolvedInjectedMember );

        return GetCorrespondingSymbolForResolvedSymbol( referencedSymbol, symbol );
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
            case (IMethodSymbol { MethodKind: MethodKind.Constructor }, IMethodSymbol { MethodKind: MethodKind.Constructor }):
            case (IMethodSymbol { MethodKind: MethodKind.StaticConstructor }, IMethodSymbol { MethodKind: MethodKind.Ordinary }):
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
                throw new AssertionFailedException( $"Unexpected combination: ('{referencedSymbol}', '{resolvedSymbol}')" );
        }
    }

    private record struct OverrideIndex( MemberLayerIndex Index, InjectedMember Override );

    private sealed class ConditionalAccessExpressionWalker : SafeSyntaxWalker
    {
        private ConditionalAccessExpressionSyntax? _context;

        public MemberBindingExpressionSyntax? FoundName { get; private set; }

        public override void VisitConditionalAccessExpression( ConditionalAccessExpressionSyntax node )
        {
            if ( this._context == null )
            {
                this._context = node;

                this.Visit( node.WhenNotNull );

                this._context = null;
            }
        }

        public override void VisitMemberBindingExpression( MemberBindingExpressionSyntax node )
        {
            if ( this._context != null )
            {
                this.FoundName = node;
            }
        }
    }
}