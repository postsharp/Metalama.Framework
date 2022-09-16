// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        private class AspectReferenceCollector
        {
            private readonly PartialCompilation _intermediateCompilation;
            private readonly LinkerIntroductionRegistry _introductionRegistry;
            private readonly AspectReferenceResolver _referenceResolver;

            public AspectReferenceCollector(
                PartialCompilation intermediateCompilation,
                LinkerIntroductionRegistry introductionRegistry,
                AspectReferenceResolver referenceResolver )
            {
                this._intermediateCompilation = intermediateCompilation;
                this._introductionRegistry = introductionRegistry;
                this._referenceResolver = referenceResolver;
            }

            public IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> Run()
            {
                Dictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> aspectReferences = new();

                // Add implicit references going from final semantic to the last override.
                foreach ( var overriddenMember in this._introductionRegistry.GetOverriddenMembers() )
                {
                    switch ( overriddenMember )
                    {
                        case IMethodSymbol method:
                            AddImplicitReference(
                                method,
                                method,
                                this._introductionRegistry.GetLastOverride( method ),
                                AspectReferenceTargetKind.Self );

                            break;

                        case IPropertySymbol property:
                            if ( property.GetMethod != null )
                            {
                                AddImplicitReference(
                                    property.GetMethod,
                                    property,
                                    this._introductionRegistry.GetLastOverride( property ),
                                    AspectReferenceTargetKind.PropertyGetAccessor );
                            }

                            if ( property.SetMethod != null )
                            {
                                AddImplicitReference(
                                    property.SetMethod,
                                    property,
                                    this._introductionRegistry.GetLastOverride( property ),
                                    AspectReferenceTargetKind.PropertySetAccessor );
                            }

                            break;

                        case IEventSymbol @event:
                            AddImplicitReference(
                                @event.AddMethod.AssertNotNull(),
                                @event,
                                this._introductionRegistry.GetLastOverride( @event ),
                                AspectReferenceTargetKind.EventAddAccessor );

                            AddImplicitReference(
                                @event.RemoveMethod.AssertNotNull(),
                                @event,
                                this._introductionRegistry.GetLastOverride( @event ),
                                AspectReferenceTargetKind.EventRemoveAccessor );

                            break;
                    }

                    void AddImplicitReference(
                        IMethodSymbol containingSymbol,
                        ISymbol target,
                        ISymbol lastOverrideSymbol,
                        AspectReferenceTargetKind targetKind )
                    {
                        // Implicit reference pointing from final semantic to the last override.
                        var containingSemantic = containingSymbol.ToSemantic( IntermediateSymbolSemanticKind.Final );

                        var sourceNode =
                            containingSymbol.GetPrimaryDeclaration() switch
                            {
                                MethodDeclarationSyntax method => method.Body ?? (SyntaxNode?) method.ExpressionBody ?? method,
                                DestructorDeclarationSyntax destructor => destructor.Body
                                                                          ?? (SyntaxNode?) destructor.ExpressionBody ?? throw new AssertionFailedException(),
                                OperatorDeclarationSyntax @operator => @operator.Body
                                                                       ?? (SyntaxNode?) @operator.ExpressionBody ?? throw new AssertionFailedException(),
                                ConversionOperatorDeclarationSyntax conversionOperator => conversionOperator.Body
                                                                                          ?? (SyntaxNode?) conversionOperator.ExpressionBody
                                                                                          ?? throw new AssertionFailedException(),
                                AccessorDeclarationSyntax accessor => accessor.Body
                                                                      ?? (SyntaxNode?) accessor.ExpressionBody
                                                                      ?? accessor ?? throw new AssertionFailedException(),
                                VariableDeclaratorSyntax declarator => declarator ?? throw new AssertionFailedException(),
                                ArrowExpressionClauseSyntax arrowExpressionClause => arrowExpressionClause,
                                ParameterSyntax { Parent: ParameterListSyntax { Parent: RecordDeclarationSyntax } } recordParameter => recordParameter,
                                _ => throw new AssertionFailedException()
                            };

                        aspectReferences.Add(
                            containingSemantic,
                            new[]
                            {
                                new ResolvedAspectReference(
                                    containingSemantic,
                                    target,
                                    lastOverrideSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default ),
                                    sourceNode,
                                    targetKind,
                                    isInlineable: true )
                            } );
                    }
                }

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

                return aspectReferences;

                void AnalyzeOverriddenBody( IMethodSymbol symbol )
                {
                    var semantic = symbol.ToSemantic( IntermediateSymbolSemanticKind.Default );
                    aspectReferences[semantic] = ImmutableArray<ResolvedAspectReference>.Empty;
                }

                void AnalyzeIntroducedBody( IMethodSymbol symbol )
                {
                    var semantic = symbol.ToSemantic( IntermediateSymbolSemanticKind.Default );
                    var syntax = symbol.GetPrimaryDeclaration().AssertNotNull();

                    var aspectReferenceCollector = new AspectReferenceWalker(
                        this._referenceResolver,
                        this._intermediateCompilation.Compilation.GetSemanticModel( syntax.SyntaxTree ),
                        symbol );

                    aspectReferenceCollector.Visit( syntax );

                    aspectReferences[semantic] = aspectReferenceCollector.AspectReferences.ToImmutableArray();
                }
            }
        }
    }
}