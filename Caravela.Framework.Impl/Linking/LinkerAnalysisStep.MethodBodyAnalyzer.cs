// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerAnalysisStep
    {
        private class MethodBodyAnalyzer
        {
            private readonly PartialCompilation _intermediateCompilation;
            private readonly LinkerIntroductionRegistry _introductionRegistry;
            private readonly AspectReferenceResolver _referenceResolver;

            public MethodBodyAnalyzer(
                PartialCompilation intermediateCompilation,
                LinkerIntroductionRegistry introductionRegistry,
                AspectReferenceResolver referenceResolver )
            {
                this._intermediateCompilation = intermediateCompilation;
                this._introductionRegistry = introductionRegistry;
                this._referenceResolver = referenceResolver;
            }

            public IReadOnlyDictionary<ISymbol, MethodBodyAnalysisResult> GetMethodBodyAnalysisResults()
            {
                Dictionary<ISymbol, MethodBodyAnalysisResult> bodyAnalysisResults = new( SymbolEqualityComparer.Default );

                // TODO: Do this on demand in analysis registry (provide the implementing class to the registry, let the registry manage the cache).
                // Analyze introduced method bodies.
                foreach ( var introducedMember in this._introductionRegistry.GetIntroducedMembers() )
                {
                    var symbol = this._introductionRegistry.GetSymbolForIntroducedMember( introducedMember );

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

                        case IFieldSymbol:
                            // NOP.
                            break;

                        default:
                            throw new NotSupportedException();

                            // var declarationSyntax = (MethodDeclarationSyntax) symbol.DeclaringSyntaxReferences.Single().GetSyntax();
                            // ControlFlowGraph cfg = ControlFlowGraph.Create( declarationSyntax, this._intermediateCompilation.GetSemanticModel( declarationSyntax.SyntaxTree ) );
                    }
                }

                foreach ( var symbol in this._introductionRegistry.GetOverriddenMembers() )
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

                return bodyAnalysisResults;

                void AnalyzeOverriddenBody( IMethodSymbol symbol )
                {
                    bodyAnalysisResults[symbol] = new MethodBodyAnalysisResult( Array.Empty<ResolvedAspectReference>() );
                }

                void AnalyzeIntroducedBody( IMethodSymbol symbol )
                {
                    var syntax = symbol.GetPrimaryDeclaration().AssertNotNull();

                    var aspectReferenceCollector = new AspectReferenceWalker(
                        this._referenceResolver,
                        this._intermediateCompilation.Compilation.GetSemanticModel( syntax.SyntaxTree ),
                        symbol );

                    aspectReferenceCollector.Visit( syntax );

                    bodyAnalysisResults[symbol] = new MethodBodyAnalysisResult( aspectReferenceCollector.AspectReferences );
                }
            }
        }
    }
}