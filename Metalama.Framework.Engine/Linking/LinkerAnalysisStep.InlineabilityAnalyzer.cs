// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Linking.Inlining;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        public class InlineabilityAnalyzer
        {
            private readonly PartialCompilation _intermediateCompilation;
            private readonly LinkerIntroductionRegistry _introductionRegistry;
            private readonly IReadOnlyList<IntermediateSymbolSemantic> _reachableSymbolSemantics;
            private readonly InlinerProvider _inlinerProvider;
            private readonly IReadOnlyDictionary<AspectReferenceTarget, IReadOnlyList<ResolvedAspectReference>> _aspectReferenceIndex;

            public InlineabilityAnalyzer(
                PartialCompilation intermediateCompilation,
                LinkerIntroductionRegistry introductionRegistry,
                IReadOnlyList<IntermediateSymbolSemantic> reachableSymbolSemantics,
                InlinerProvider inlinerProvider,
                IReadOnlyDictionary<AspectReferenceTarget, IReadOnlyList<ResolvedAspectReference>> aspectReferenceIndex )
            {
                this._intermediateCompilation = intermediateCompilation;
                this._introductionRegistry = introductionRegistry;
                this._reachableSymbolSemantics = reachableSymbolSemantics;
                this._inlinerProvider = inlinerProvider;
                this._aspectReferenceIndex = aspectReferenceIndex;
            }

            public IEnumerable<SymbolInliningSpecification> GetInlineableSymbols()
            {
                // Go through reachable symbols and try to determine whether they are inlineable.
                foreach ( var reachableSymbolSemantic in this._reachableSymbolSemantics )
                {
                    if ( this.TryInline( reachableSymbolSemantic, out var inliningSpecification ) )
                    {
                        yield return inliningSpecification;
                    }
                }
            }

            private IReadOnlyList<ResolvedAspectReference> GetAspectReferences(
                IntermediateSymbolSemantic semantic,
                AspectReferenceTargetKind targetKind = AspectReferenceTargetKind.Self )
            {
                if ( !this._aspectReferenceIndex.TryGetValue(
                        new AspectReferenceTarget( semantic.Symbol, semantic.Kind, targetKind ),
                        out var containedReferences ) )
                {
                    return Array.Empty<ResolvedAspectReference>();
                }

                return containedReferences;
            }

            private bool TryInline( IntermediateSymbolSemantic semantic, [NotNullWhen( true )] out SymbolInliningSpecification? inliningSpecification )
            {
                switch ( semantic.Symbol )
                {
                    case IMethodSymbol:
                        return this.TryInlineMethod( semantic.ToTyped<IMethodSymbol>(), out inliningSpecification );

                    case IPropertySymbol:
                        return this.TryInlineProperty( semantic.ToTyped<IPropertySymbol>(), out inliningSpecification );

                    case IEventSymbol:
                        return this.TryInlineEvent( semantic.ToTyped<IEventSymbol>(), out inliningSpecification );

                    case IFieldSymbol:
                        inliningSpecification = null;

                        return false;

                    default:
                        throw new AssertionFailedException();
                }
            }

            private bool TryInlineMethod(
                IntermediateSymbolSemantic<IMethodSymbol> semantic,
                [NotNullWhen( true )] out SymbolInliningSpecification? inliningSpecification )
            {
                switch ( semantic.Symbol.MethodKind )
                {
                    case MethodKind.Ordinary:
                    case MethodKind.ExplicitInterfaceImplementation:
                    case MethodKind.Destructor:
                    case MethodKind.UserDefinedOperator:
                    case MethodKind.Conversion:
                        if ( semantic.Symbol.GetDeclarationFlags().HasFlag( LinkerDeclarationFlags.NotInlineable )
                             || semantic.Kind == IntermediateSymbolSemanticKind.Final )
                        {
                            inliningSpecification = null;

                            return false;
                        }

                        if ( this._introductionRegistry.IsLastOverride( semantic.Symbol ) )
                        {
                            // Last overrides should be inlined if not marked as not-inlineable.
                            inliningSpecification = new SymbolInliningSpecification( semantic );

                            return true;
                        }

                        var aspectReferences = this.GetAspectReferences( semantic );

                        if ( aspectReferences.Count != 1 )
                        {
                            inliningSpecification = null;

                            return false;
                        }

                        if ( aspectReferences.Count != 0 && this.TryGetInliner( aspectReferences[0], out var inliner ) )
                        {
                            inliningSpecification = new SymbolInliningSpecification(
                                semantic,
                                new KeyValuePair<ResolvedAspectReference, Inliner>( aspectReferences[0], inliner ) );

                            return true;
                        }

                        inliningSpecification = null;

                        return false;

                    case MethodKind.EventAdd:
                    case MethodKind.EventRemove:
                    case MethodKind.PropertyGet:
                    case MethodKind.PropertySet:
                        // Accessor methods are not inlineable by themselves, but always through the containing event/property.
                        inliningSpecification = null;

                        return false;

                    default:
                        throw new AssertionFailedException();
                }
            }

            private bool TryInlineProperty(
                IntermediateSymbolSemantic<IPropertySymbol> semantic,
                [NotNullWhen( true )] out SymbolInliningSpecification? inliningSpecification )
            {
                if ( semantic.Symbol.GetDeclarationFlags().HasFlag( LinkerDeclarationFlags.NotInlineable )
                     || semantic.Kind == IntermediateSymbolSemanticKind.Final )
                {
                    inliningSpecification = null;

                    return false;
                }

                if ( this._introductionRegistry.IsLastOverride( semantic.Symbol ) )
                {
                    // Last overrides should be inlined if not marked as not-inlineable.
                    inliningSpecification = new SymbolInliningSpecification( semantic );

                    return true;
                }

                var selfAspectReferences = this.GetAspectReferences( semantic );
                var getAspectReferences = this.GetAspectReferences( semantic, AspectReferenceTargetKind.PropertyGetAccessor );
                var setAspectReferences = this.GetAspectReferences( semantic, AspectReferenceTargetKind.PropertySetAccessor );

                if ( selfAspectReferences.Count > 0 )
                {
                    // TODO: We may need to deal with this case.
                    inliningSpecification = null;

                    return false;
                }

                if ( getAspectReferences.Count > 1 || setAspectReferences.Count > 1
                                                   || (getAspectReferences.Count == 0 && setAspectReferences.Count == 0) )
                {
                    inliningSpecification = null;

                    return false;
                }

                Inliner? getterInliner = null;
                Inliner? setterInliner = null;

                if ( (semantic.Symbol.GetMethod == null || getAspectReferences.Count == 0 || this.TryGetInliner( getAspectReferences[0], out getterInliner ))
                     && (semantic.Symbol.SetMethod == null || setAspectReferences.Count == 0
                                                           || this.TryGetInliner( setAspectReferences[0], out setterInliner )) )
                {
                    if ( getterInliner == null )
                    {
                        inliningSpecification = new SymbolInliningSpecification(
                            semantic,
                            new KeyValuePair<ResolvedAspectReference, Inliner>( setAspectReferences[0], setterInliner.AssertNotNull() ) );
                    }
                    else if ( setterInliner == null )
                    {
                        inliningSpecification = new SymbolInliningSpecification(
                            semantic,
                            new KeyValuePair<ResolvedAspectReference, Inliner>( getAspectReferences[0], getterInliner.AssertNotNull() ) );
                    }
                    else
                    {
                        inliningSpecification = new SymbolInliningSpecification(
                            semantic,
                            new KeyValuePair<ResolvedAspectReference, Inliner>( getAspectReferences[0], getterInliner.AssertNotNull() ),
                            new KeyValuePair<ResolvedAspectReference, Inliner>( setAspectReferences[0], setterInliner.AssertNotNull() ) );
                    }

                    return true;
                }

                inliningSpecification = null;

                return false;
            }

            private bool TryInlineEvent(
                IntermediateSymbolSemantic<IEventSymbol> semantic,
                [NotNullWhen( true )] out SymbolInliningSpecification? inliningSpecification )
            {
                if ( semantic.Symbol.GetDeclarationFlags().HasFlag( LinkerDeclarationFlags.NotInlineable )
                     || semantic.Kind == IntermediateSymbolSemanticKind.Final )
                {
                    inliningSpecification = null;

                    return false;
                }

                if ( this._introductionRegistry.IsLastOverride( semantic.Symbol ) )
                {
                    // Last overrides should be inlined if not marked as not-inlineable.
                    inliningSpecification = new SymbolInliningSpecification( semantic );

                    return true;
                }

                var selfAspectReferences = this.GetAspectReferences( semantic );
                var addAspectReferences = this.GetAspectReferences( semantic, AspectReferenceTargetKind.EventAddAccessor );
                var removeAspectReferences = this.GetAspectReferences( semantic, AspectReferenceTargetKind.EventRemoveAccessor );

                if ( selfAspectReferences.Count > 0 )
                {
                    // TODO: We may need to deal with this case.
                    inliningSpecification = null;

                    return false;
                }

                if ( addAspectReferences.Count > 1 || removeAspectReferences.Count > 1
                                                   || (addAspectReferences.Count == 0 && removeAspectReferences.Count == 0) )
                {
                    inliningSpecification = null;

                    return false;
                }

                Inliner? adderInliner = null;
                Inliner? removerInliner = null;

                if ( (addAspectReferences.Count == 0 || this.TryGetInliner( addAspectReferences[0], out adderInliner ))
                     && (removeAspectReferences.Count == 0 || this.TryGetInliner( removeAspectReferences[0], out removerInliner )) )
                {
                    var selectedInliners = new List<KeyValuePair<ResolvedAspectReference, Inliner>>();

                    if ( adderInliner != null )
                    {
                        selectedInliners.Add( new KeyValuePair<ResolvedAspectReference, Inliner>( addAspectReferences[0], adderInliner ) );
                    }

                    if ( removerInliner != null )
                    {
                        selectedInliners.Add( new KeyValuePair<ResolvedAspectReference, Inliner>( removeAspectReferences[0], removerInliner ) );
                    }

                    inliningSpecification = new SymbolInliningSpecification( semantic, selectedInliners.ToArray() );

                    return true;
                }

                inliningSpecification = null;

                return false;
            }

            private bool TryGetInliner( ResolvedAspectReference aspectReference, [NotNullWhen( true )] out Inliner? inliner )
            {
                var semanticModel = this._intermediateCompilation.Compilation.GetSemanticModel( aspectReference.Expression.SyntaxTree );

                if ( !aspectReference.Specification.Flags.HasFlag( AspectReferenceFlags.Inlineable ) )
                {
                    inliner = null;

                    return false;
                }

                return this._inlinerProvider.TryGetInliner( aspectReference, semanticModel, out inliner );
            }
        }
    }
}