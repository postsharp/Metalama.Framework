﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            var referenceCounters = new Dictionary<SymbolVersion, int>();
            var methodBodyInfos = new Dictionary<ISymbol, MemberAnalysisResult>();

            var layersId = input.OrderedAspectLayers.Select( x => x.AspectLayerId ).ToArray();

            foreach ( var syntaxTree in input.IntermediateCompilation.SyntaxTrees )
            {
                foreach ( var referencingNode in syntaxTree.GetRoot().GetAnnotatedNodes( AspectReferenceAnnotationExtensions.AnnotationKind ) )
                {
                    if (!referencingNode.TryGetAspectReference( out var linkerAnnotation ))
                    {
                        throw new AssertionFailedException();
                    }

                    AspectLayerId? targetLayerId;

                    // Determine which version of the semantic is being invoked.
                    switch ( linkerAnnotation.Order )
                    {
                        case AspectReferenceOrder.Base: // TODO:
                            var currentLayerIndex = Array.IndexOf( layersId, linkerAnnotation.AspectLayerId );
                            Invariant.Assert( currentLayerIndex >= 0 );

                            targetLayerId = currentLayerIndex == 0 ? null : layersId[currentLayerIndex - 1];

                            break;

                        case AspectReferenceOrder.Original: // Original
                            targetLayerId = null;

                            break;

                        case AspectReferenceOrder.Final:
                            targetLayerId = layersId[layersId.Length - 1];

                            break;

                        case AspectReferenceOrder.Default: // Next one.
                            targetLayerId = linkerAnnotation.AspectLayerId;

                            break;

                        default:
                            throw new AssertionFailedException();
                    }

                    // Increment the usage count.
                    var symbolInfo = input.IntermediateCompilation.Compilation.GetSemanticModel( syntaxTree ).GetSymbolInfo( referencingNode );

                    if ( symbolInfo.Symbol == null )
                    {
                        continue;
                    }

                    var symbolVersion = new SymbolVersion( symbolInfo.Symbol.AssertNotNull(), targetLayerId, linkerAnnotation.TargetKind );

                    _ = referenceCounters.TryGetValue( symbolVersion, out var counter );
                    referenceCounters[symbolVersion] = counter + 1;
                }
            }

            // TODO: Do this on demand in analysis registry (provide the implementing class to the registry, let the registry manage the cache).
            // Analyze introduced method bodies.
            foreach ( var introducedMember in input.IntroductionRegistry.GetIntroducedMembers() )
            {
                var symbol = input.IntroductionRegistry.GetSymbolForIntroducedMember( introducedMember );

                switch ( symbol )
                {
                    case IMethodSymbol methodSymbol:
                        // TODO: partial methods.
                        var methodBodyVisitor = new MethodBodyWalker();
                        methodBodyVisitor.Visit( methodSymbol.DeclaringSyntaxReferences.Single().GetSyntax() );

                        methodBodyInfos[methodSymbol] = new MemberAnalysisResult(
                            methodSymbol.ReturnsVoid ? methodBodyVisitor.ReturnStatementCount == 0 : methodBodyVisitor.ReturnStatementCount <= 1 );

                        break;

                    case IPropertySymbol propertySymbol:
                        if ( propertySymbol.GetMethod != null )
                        {
                            var getterBodyVisitor = new MethodBodyWalker();
                            getterBodyVisitor.Visit( propertySymbol.GetMethod.DeclaringSyntaxReferences.Single().GetSyntax() );

                            methodBodyInfos[propertySymbol.GetMethod] = new MemberAnalysisResult( getterBodyVisitor.ReturnStatementCount <= 1 );
                        }

                        if ( propertySymbol.SetMethod != null )
                        {
                            var setterBodyVisitor = new MethodBodyWalker();
                            setterBodyVisitor.Visit( propertySymbol.SetMethod.DeclaringSyntaxReferences.Single().GetSyntax() );

                            methodBodyInfos[propertySymbol.SetMethod] = new MemberAnalysisResult( setterBodyVisitor.ReturnStatementCount == 0 );
                        }

                        break;

                    case IEventSymbol eventSymbol:

                        var addBodySyntax = eventSymbol.AddMethod?.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax();

                        if ( addBodySyntax != null )
                        {
                            var addBodyVisitor = new MethodBodyWalker();
                            addBodyVisitor.Visit( addBodySyntax );

                            methodBodyInfos[eventSymbol.AddMethod!] = new MemberAnalysisResult( addBodyVisitor.ReturnStatementCount == 0 );
                        }

                        var removeBodySyntax = eventSymbol.RemoveMethod?.DeclaringSyntaxReferences.SingleOrDefault()?.GetSyntax();

                        if ( removeBodySyntax != null )
                        {
                            var removeBodyVisitor = new MethodBodyWalker();
                            removeBodyVisitor.Visit( removeBodySyntax );

                            methodBodyInfos[eventSymbol.RemoveMethod!] = new MemberAnalysisResult( removeBodyVisitor.ReturnStatementCount == 0 );
                        }

                        break;

                    default:
                        throw new InvalidOperationException( $"{symbol.Kind}" );
                }

                // var declarationSyntax = (MethodDeclarationSyntax) symbol.DeclaringSyntaxReferences.Single().GetSyntax();
                // ControlFlowGraph cfg = ControlFlowGraph.Create( declarationSyntax, this._intermediateCompilation.GetSemanticModel( declarationSyntax.SyntaxTree ) );
            }

            foreach ( var symbol in input.IntroductionRegistry.GetOverriddenMembers() )
            {
                switch ( symbol )
                {
                    case IMethodSymbol methodSymbol:
                        AnalyzeMethodBody( methodBodyInfos, methodSymbol );

                        break;

                    case IPropertySymbol propertySymbol:
                        if ( propertySymbol.GetMethod != null )
                        {
                            AnalyzeMethodBody( methodBodyInfos, propertySymbol.GetMethod );
                        }

                        if ( propertySymbol.SetMethod != null )
                        {
                            AnalyzeMethodBody( methodBodyInfos, propertySymbol.SetMethod );
                        }

                        break;

                    case IEventSymbol eventSymbol:
                        if ( eventSymbol.AddMethod != null )
                        {
                            AnalyzeMethodBody( methodBodyInfos, eventSymbol.AddMethod );
                        }

                        if ( eventSymbol.RemoveMethod != null )
                        {
                            AnalyzeMethodBody( methodBodyInfos, eventSymbol.RemoveMethod );
                        }

                        break;

                    default:
                        throw new NotSupportedException();
                }
            }

            var analysisRegistry = new LinkerAnalysisRegistry( input.IntroductionRegistry, input.OrderedAspectLayers, referenceCounters, methodBodyInfos );

            return new LinkerAnalysisStepOutput( input.Diagnostics, input.IntermediateCompilation, analysisRegistry, new AspectReferenceResolver( input.IntroductionRegistry, input.OrderedAspectLayers ) );
        }

        private static void AnalyzeMethodBody( Dictionary<ISymbol, MemberAnalysisResult> methodBodyInfos, IMethodSymbol symbol )
        {
            var syntax = symbol.DeclaringSyntaxReferences.Single().GetSyntax();

            switch ( syntax )
            {
                case MethodDeclarationSyntax methodDecl:
                    if ( methodDecl.Body != null )
                    {
                        var methodBodyWalker = new MethodBodyWalker();
                        methodBodyWalker.Visit( methodDecl.Body );

                        methodBodyInfos[symbol] = new MemberAnalysisResult(
                            symbol.ReturnsVoid ? methodBodyWalker.ReturnStatementCount == 0 : methodBodyWalker.ReturnStatementCount <= 1 );
                    }
                    else if ( methodDecl.ExpressionBody != null )
                    {
                        methodBodyInfos[symbol] = new MemberAnalysisResult( true );
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }

                    break;

                case AccessorDeclarationSyntax accessorDecl:
                    if ( accessorDecl.Body != null )
                    {
                        var methodBodyWalker = new MethodBodyWalker();
                        methodBodyWalker.Visit( accessorDecl.Body );

                        methodBodyInfos[symbol] = new MemberAnalysisResult(
                            symbol.ReturnsVoid ? methodBodyWalker.ReturnStatementCount == 0 : methodBodyWalker.ReturnStatementCount <= 1 );
                    }
                    else if ( accessorDecl.ExpressionBody != null )
                    {
                        methodBodyInfos[symbol] = new MemberAnalysisResult( true );
                    }
                    else
                    {
                        // Auto property?.
                        methodBodyInfos[symbol] = new MemberAnalysisResult( true );
                    }

                    break;

                case ArrowExpressionClauseSyntax _:
                    methodBodyInfos[symbol] = new MemberAnalysisResult( true );

                    break;

                default:
                    throw new NotSupportedException();
            }
        }
    }
}