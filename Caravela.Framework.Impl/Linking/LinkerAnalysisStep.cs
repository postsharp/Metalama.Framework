// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Analysis step of the linker, main goal of which is to produce LinkerAnalysisRegistry.
    /// </summary>
    internal partial class LinkerAnalysisStep : AspectLinkerPipelineStep<LinkerIntroductionStepOutput, LinkerAnalysisStepOutput>
    {
        public static LinkerAnalysisStep Instance { get; } = new();

        private LinkerAnalysisStep() { }

        public override LinkerAnalysisStepOutput Execute( LinkerIntroductionStepOutput input )
        {
            Dictionary<ISymbol, MethodBodyAnalysisResult> methodBodyAnalysisResults = new();
            Dictionary<(ISymbol Symbol, ResolvedAspectReferenceSemantic Semantic, AspectReferenceTargetKind TargetKind), List<ResolvedAspectReference>> aspectReferences = new();
            var referenceResolver = new AspectReferenceResolver( input.IntroductionRegistry, input.OrderedAspectLayers );

            var layersId = input.OrderedAspectLayers.Select( x => x.AspectLayerId ).ToArray();

            // TODO: Do this on demand in analysis registry (provide the implementing class to the registry, let the registry manage the cache).
            // Analyze introduced method bodies.
            foreach ( var introducedMember in input.IntroductionRegistry.GetIntroducedMembers() )
            {
                var symbol = input.IntroductionRegistry.GetSymbolForIntroducedMember( introducedMember );

                switch ( symbol )
                {
                    case IMethodSymbol methodSymbol:
                        AnalyzeIntroducedBody( methodSymbol );

                        break;

                    case IPropertySymbol propertySymbol:
                        if ( propertySymbol.GetMethod != null )
                        {
                            AnalyzeIntroducedBody( propertySymbol.GetMethod );
                        }

                        if ( propertySymbol.SetMethod != null )
                        {
                            AnalyzeIntroducedBody( propertySymbol.SetMethod );
                        }

                        break;

                    case IEventSymbol eventSymbol:
                        AnalyzeIntroducedBody( eventSymbol.AddMethod.AssertNotNull() );
                        AnalyzeIntroducedBody( eventSymbol.RemoveMethod.AssertNotNull() );

                        break;

                    default:
                        throw new NotSupportedException();

                        // var declarationSyntax = (MethodDeclarationSyntax) symbol.DeclaringSyntaxReferences.Single().GetSyntax();
                        // ControlFlowGraph cfg = ControlFlowGraph.Create( declarationSyntax, this._intermediateCompilation.GetSemanticModel( declarationSyntax.SyntaxTree ) );
                }
            }

            foreach ( var symbol in input.IntroductionRegistry.GetOverriddenMembers() )
            {
                switch ( symbol )
                {
                    case IMethodSymbol methodSymbol:
                        AnalyzeOverriddenBody( methodSymbol );

                        break;

                    case IPropertySymbol propertySymbol:
                        if ( propertySymbol.GetMethod != null )
                        {
                            AnalyzeOverriddenBody( propertySymbol.GetMethod );
                        }

                        if ( propertySymbol.SetMethod != null )
                        {
                            AnalyzeOverriddenBody( propertySymbol.SetMethod );
                        }

                        break;

                    case IEventSymbol eventSymbol:
                        AnalyzeOverriddenBody( eventSymbol.AddMethod.AssertNotNull() );
                        AnalyzeOverriddenBody( eventSymbol.RemoveMethod.AssertNotNull() );

                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            var analysisRegistry = new LinkerAnalysisRegistry( input.IntroductionRegistry, methodBodyAnalysisResults, aspectReferences.ToDictionary( x => x.Key, x => (IReadOnlyList<ResolvedAspectReference>)x.Value) );

            return new LinkerAnalysisStepOutput( input.Diagnostics, input.IntermediateCompilation, analysisRegistry, referenceResolver );

            void AnalyzeOverriddenBody( IMethodSymbol symbol )
            {
                var syntax = symbol.GetPrimaryDeclaration();
                var returnStatementCounter = new ReturnStatementCountingWalker();
                returnStatementCounter.Visit( syntax );

                methodBodyAnalysisResults[symbol] = new MethodBodyAnalysisResult(                    
                    Array.Empty<ResolvedAspectReference>(),
                    symbol.ReturnsVoid ? returnStatementCounter.ReturnStatementCount == 0 : returnStatementCounter.ReturnStatementCount <= 1 );
            }

            void AnalyzeIntroducedBody( IMethodSymbol symbol )
            {
                var syntax = symbol.GetPrimaryDeclaration().AssertNotNull();
                var returnStatementCounter = new ReturnStatementCountingWalker();
                returnStatementCounter.Visit( syntax );
                var aspectReferenceCollector = new AspectReferenceWalker( referenceResolver, input.IntermediateCompilation.Compilation.GetSemanticModel( syntax.SyntaxTree ), symbol );                
                aspectReferenceCollector.Visit( syntax );

                methodBodyAnalysisResults[symbol] = new MethodBodyAnalysisResult(
                    aspectReferenceCollector.AspectReferences,
                    symbol.ReturnsVoid ? returnStatementCounter.ReturnStatementCount == 0 : returnStatementCounter.ReturnStatementCount <= 1 );

                foreach (var aspectReference in aspectReferenceCollector.AspectReferences)
                {
                    if (!aspectReferences.TryGetValue( (aspectReference.ResolvedSymbol, aspectReference.Semantic, aspectReference.Specification.TargetKind), out var list))
                    {
                        aspectReferences[(aspectReference.ResolvedSymbol, aspectReference.Semantic, aspectReference.Specification.TargetKind)] = list = new List<ResolvedAspectReference>();
                    }

                    list.Add( aspectReference );
                }
            }
        }
    }
}