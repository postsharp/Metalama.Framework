// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Linking.Inlining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        private class InliningAlgorithm
        {
            private readonly LinkerIntroductionRegistry _introductionRegistry;
            private readonly IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> _aspectReferencesByContainingSemantic;
            private readonly IReadOnlyList<IntermediateSymbolSemantic> _reachableSemantics;
            private readonly HashSet<IntermediateSymbolSemantic> _inlinedSemantics;
            private readonly IReadOnlyDictionary<ResolvedAspectReference, Inliner> _inlinedReferences;
            private readonly IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, SemanticBodyAnalysisResult> _bodyAnalysisResults;

            public InliningAlgorithm( 
                LinkerIntroductionRegistry introductionRegistry,
                IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> aspectReferencesByContainingSemantic, 
                IReadOnlyList<IntermediateSymbolSemantic> reachableSemantics, 
                IReadOnlyList<IntermediateSymbolSemantic> inlinedSemantics, 
                IReadOnlyDictionary<ResolvedAspectReference, Inliner> inlinedReferences, 
                IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, SemanticBodyAnalysisResult> bodyAnalysisResults )
            {
                this._introductionRegistry = introductionRegistry;
                this._aspectReferencesByContainingSemantic = aspectReferencesByContainingSemantic;
                this._reachableSemantics = reachableSemantics;
                this._inlinedSemantics = new HashSet<IntermediateSymbolSemantic>(inlinedSemantics);
                this._inlinedReferences = inlinedReferences;
                this._bodyAnalysisResults = bodyAnalysisResults;
            }

            internal IReadOnlyList<InliningSpecification> Run()
            {
                var inliningSpecifications = new List<InliningSpecification>();

                foreach ( var semantic in this._reachableSemantics )
                {
                    if (semantic.Symbol is not IMethodSymbol)
                    {
                        continue;
                    }

                    if ( this._inlinedSemantics.Contains( semantic ) )
                    {
                        // Inlined semantics are not destinations of inlining.
                        continue;
                    }

                    // Allocate labels for the whole final method. This would be a problem only if we allow inlining multiple references in one body.
                    var inliningContext = new InliningAnalysisContext();

                    VisitSemantic( semantic, inliningContext );
                }

                return inliningSpecifications;

                void VisitSemantic( IntermediateSymbolSemantic semantic, InliningAnalysisContext context )
                {
                    // Follow edges between method groups and accessors.
                    switch ( semantic.Symbol )
                    {
                        case IMethodSymbol method:
                            VisitSemanticBody(
                                method.ToSemantic( semantic.Kind ),
                                method.ToSemantic( semantic.Kind ),
                                context );

                            break;

                        case IPropertySymbol property:
                            if ( property.GetMethod != null )
                            {
                                VisitSemanticBody( 
                                    property.GetMethod.ToSemantic( semantic.Kind ),
                                    property.GetMethod.ToSemantic( semantic.Kind ),
                                    context );
                            }

                            if ( property.SetMethod != null )
                            {
                                VisitSemanticBody( 
                                    property.SetMethod.ToSemantic( semantic.Kind ),
                                    property.SetMethod.ToSemantic( semantic.Kind ),
                                    context );
                            }

                            break;

                        case IEventSymbol @event:
                            VisitSemanticBody( 
                                @event.AddMethod.AssertNotNull().ToSemantic( semantic.Kind ),
                                @event.AddMethod.AssertNotNull().ToSemantic( semantic.Kind ),
                                context );

                            VisitSemanticBody( 
                                @event.RemoveMethod.AssertNotNull().ToSemantic( semantic.Kind ),
                                @event.RemoveMethod.AssertNotNull().ToSemantic( semantic.Kind ),
                                context );

                            break;
                    }
                }

                void VisitSemanticBody( IntermediateSymbolSemantic<IMethodSymbol> destinationSemantic, IntermediateSymbolSemantic<IMethodSymbol> currentSemantic, InliningAnalysisContext context )
                {
                    if ( !this._aspectReferencesByContainingSemantic.TryGetValue( currentSemantic, out var aspectReferences ) )
                    {
                        return;
                    }

                    // Go through all references and recurse into inlined bodies.
                    foreach ( var aspectReference in aspectReferences )
                    {
                        if ( this._inlinedReferences.TryGetValue( aspectReference, out var inliner ) )
                        {
                            var targetSemantic = aspectReference.ResolvedSemanticBody;
                            var info = inliner.GetInliningAnalysisInfo( context, aspectReference );

                            if ( context.UsingSimpleInlining && (info.ReplacedStatement is ReturnStatementSyntax || currentSemantic.Kind == IntermediateSymbolSemanticKind.Final))
                            {
                                // This is inlining of a return statement while we can do simple inlining - no rewriting of returns is required.
                                // Or it is inlining of in the final semantic, which is a special case.
                                Invariant.Assert( info.ReturnVariableIdentifier == null );
                                
                                inliningSpecifications.Add(
                                    new InliningSpecification(
                                        destinationSemantic,
                                        context.Ordinal,
                                        context.ParentOrdinal,
                                        aspectReference,
                                        inliner,
                                        info.ReplacedStatement,
                                        true,
                                        false,
                                        null,
                                        null,
                                        targetSemantic ) );

                                VisitSemanticBody( destinationSemantic, targetSemantic, context.Recurse() );
                            }
                            else
                            {
                                if (!this._bodyAnalysisResults.TryGetValue(targetSemantic, out var bodyAnalysisResult) )
                                {
                                    throw new AssertionFailedException();
                                }

                                // Allocate return label if and only if there is a return statement, removal of which would cause change in control flow in the inlined body.
                                var returnLabelIdentifier =
                                    bodyAnalysisResult.ReturnStatements.Any( s => !s.Value.FlowsToExitIfRewritten )
                                    ? context.AllocateReturnLabel()
                                    : null;

                                inliningSpecifications.Add(
                                    new InliningSpecification(
                                        destinationSemantic,
                                        context.Ordinal,
                                        context.ParentOrdinal,
                                        aspectReference,
                                        inliner,
                                        info.ReplacedStatement,
                                        false,
                                        context.DeclaredReturnVariable,
                                        info.ReturnVariableIdentifier,
                                        returnLabelIdentifier,
                                        targetSemantic ) );

                                VisitSemanticBody( destinationSemantic, targetSemantic, context.RecurseWithComplexInlining() );
                            }
                        }
                    }
                }
            }
        }
    }
}