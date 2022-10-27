// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        private class AspectReferenceCollector
        {
            private readonly ITaskScheduler _taskScheduler;
            private readonly LinkerInjectionRegistry _injectionRegistry;
            private readonly AspectReferenceResolver _referenceResolver;
            private readonly SemanticModelProvider _semanticModelProvider;

            public AspectReferenceCollector(
                IServiceProvider serviceProvider,
                PartialCompilation intermediateCompilation,
                LinkerInjectionRegistry injectionRegistry,
                AspectReferenceResolver referenceResolver )
            {
                this._semanticModelProvider = intermediateCompilation.Compilation.GetSemanticModelProvider();
                this._injectionRegistry = injectionRegistry;
                this._referenceResolver = referenceResolver;
                this._taskScheduler = serviceProvider.GetRequiredService<ITaskScheduler>();
            }

            public async Task<IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyCollection<ResolvedAspectReference>>> RunAsync(
                CancellationToken cancellationToken )
            {
                ConcurrentDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyCollection<ResolvedAspectReference>> aspectReferences = new();

                // Add implicit references going from final semantic to the last override.
                var overriddenMembers = this._injectionRegistry.GetOverriddenMembers().ToReadOnlyList();
                await this._taskScheduler.RunInParallelAsync( overriddenMembers, ProcessOverriddenMember, cancellationToken );

                void ProcessOverriddenMember( ISymbol overriddenMember )
                {
                    switch ( overriddenMember )
                    {
                        case IMethodSymbol method:
                            AddImplicitReference(
                                method,
                                method,
                                this._injectionRegistry.GetLastOverride( method ),
                                AspectReferenceTargetKind.Self );

                            break;

                        case IPropertySymbol property:
                            if ( property.GetMethod != null )
                            {
                                AddImplicitReference(
                                    property.GetMethod,
                                    property,
                                    this._injectionRegistry.GetLastOverride( property ),
                                    AspectReferenceTargetKind.PropertyGetAccessor );
                            }

                            if ( property.SetMethod != null )
                            {
                                AddImplicitReference(
                                    property.SetMethod,
                                    property,
                                    this._injectionRegistry.GetLastOverride( property ),
                                    AspectReferenceTargetKind.PropertySetAccessor );
                            }

                            break;

                        case IEventSymbol @event:
                            AddImplicitReference(
                                @event.AddMethod.AssertNotNull(),
                                @event,
                                this._injectionRegistry.GetLastOverride( @event ),
                                AspectReferenceTargetKind.EventAddAccessor );

                            AddImplicitReference(
                                @event.RemoveMethod.AssertNotNull(),
                                @event,
                                this._injectionRegistry.GetLastOverride( @event ),
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

                        if ( !aspectReferences.TryAdd(
                                containingSemantic,
                                new ConcurrentLinkedList<ResolvedAspectReference>()
                                {
                                    new(
                                        containingSemantic,
                                        target,
                                        lastOverrideSymbol.ToSemantic( IntermediateSymbolSemanticKind.Default ),
                                        sourceNode,
                                        sourceNode,
                                        sourceNode,
                                        targetKind,
                                        isInlineable: true )
                                } ) )
                        {
                            throw new AssertionFailedException();
                        }
                    }
                }

                // Analyze introduced method bodies.
                var injectedMembers = this._injectionRegistry.GetInjectedMembers();
                await this._taskScheduler.RunInParallelAsync( injectedMembers, ProcessInjectedMember, cancellationToken );

                void ProcessInjectedMember( LinkerInjectedMember injectedMember )
                {
                    var symbol = this._injectionRegistry.GetSymbolForInjectedMember( injectedMember );

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
                        // ControlFlowGraph cfg = ControlFlowGraph.Create( declarationSyntax, this._intermediateCompilation.GetCachedSemanticModel( declarationSyntax.SyntaxTree ) );
                    }
                }

                await this._taskScheduler.RunInParallelAsync( overriddenMembers, ProcessOverriddenMembers2, cancellationToken );

                void ProcessOverriddenMembers2( ISymbol symbol )
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
                    aspectReferences[semantic] = Array.Empty<ResolvedAspectReference>();
                }

                void AnalyzeIntroducedBody( IMethodSymbol symbol )
                {
                    var semantic = symbol.ToSemantic( IntermediateSymbolSemanticKind.Default );
                    var syntax = symbol.GetPrimaryDeclaration().AssertNotNull();

                    var aspectReferenceCollector = new AspectReferenceWalker(
                        this._referenceResolver,
                        this._semanticModelProvider.GetSemanticModel( syntax.SyntaxTree ),
                        symbol );

                    aspectReferenceCollector.Visit( syntax );

                    aspectReferences[semantic] = aspectReferenceCollector.AspectReferences.ToImmutableArray();
                }
            }
        }
    }
}