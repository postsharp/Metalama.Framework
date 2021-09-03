// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Impl.Utilities;
using FakeItEasy;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Caravela.Framework.Tests.Integration.Runners.Linker
{
    // Aspect linker tests' source use [PseudoIntroduction] and [PseudoOverride] attributes, that cause the marked declaration to be removed
    // and transformed into a pseudo-transformation that serves as a linker input. This is mainly to avoid jumping through hoops to reproduce
    // linker-specific problems with the full pipeline.
    //
    // Aspect layer order is collected from proceeds and in the order incidence, if different order is required, one can use attributes on types to force an order:
    // 
    // [LayerOrder(TestAspect1)]
    // [LayerOrder(TestAspect2)]
    // [LayerOrder(TestAspect1, TestLayer)]
    //
    // Pseudo override:
    // 
    // [PseudoOverride(Foo, TestAspect1, TestLayer)]
    // public int Foo_Override(int x, int y)
    // {
    //     return link(this.Foo(x,y));  <--- Points to overridden declaration and is rewritten to annotated method call.
    // }
    // TODO: annotation order
    //
    // Pseudo introduction - method body should be empty/return default:
    // 
    // [PseudoIntroduction(TestAspect2)]
    // public int Foo(int x, int y)
    // {
    //     return default;
    // }
    //
    // Options:
    //
    // link(this.Foo(x,y), noinline); <--- the target will never be inlined.

    internal partial class LinkerTestInputBuilder
    {
        private readonly TestRewriter _rewriter;

        public LinkerTestInputBuilder()
        {
            this._rewriter = new TestRewriter();
        }

        internal SyntaxNode ProcessSyntaxRoot( SyntaxNode syntaxRoot )
        {
            return this._rewriter.Visit( syntaxRoot );
        }

        public AspectLinkerInput ToAspectLinkerInput( PartialCompilation inputCompilation )
        {
            var initialCompilationModel = CompilationModel.CreateInitialInstance( inputCompilation );

            FinalizeTransformationFakes( this._rewriter, (CSharpCompilation) inputCompilation.Compilation, initialCompilationModel );

            var inputCompilationModel = initialCompilationModel.WithTransformations( this._rewriter.ObservableTransformations );

            var linkerInput = new AspectLinkerInput(
                inputCompilation,
                inputCompilationModel,
                this._rewriter.NonObservableTransformations,
                this._rewriter.OrderedAspectLayers.Select( ( al, i ) => new OrderedAspectLayer( i, al.AspectName, al.LayerName ) ).ToArray(),
                ArraySegment<ScopedSuppression>.Empty,
                null! );

            return linkerInput;
        }

        internal static PartialCompilation GetCleanCompilation( PartialCompilation compilation )
        {
            var rewriter = new CleaningRewriter();
            var cleanCompilation = (PartialCompilation) compilation.UpdateSyntaxTrees( ( syntaxRoot, _ ) => rewriter.Visit( syntaxRoot ).AssertNotNull() );

            return cleanCompilation;
        }

        private static void FinalizeTransformationFakes(
            TestRewriter rewriter,
            CSharpCompilation inputCompilation,
            CompilationModel initialCompilationModel )
        {
            var nodeIdToCodeElement = new Dictionary<string, IDeclaration>();

            var symbolToCodeElement = initialCompilationModel.GetContainedDeclarations()
                .Where( x => x is Declaration )
                .ToDictionary( x => ((Declaration) x).Symbol, x => x );

            var nodeIdToSyntaxNode = new Dictionary<string, SyntaxNode>();
            var syntaxNodeToSymbol = new Dictionary<SyntaxNode, ISymbol>();

            // Build lookup tables.
            foreach ( var syntaxTree in inputCompilation.SyntaxTrees )
            {
                var semanticModel = inputCompilation.GetSemanticModel( syntaxTree );

                foreach ( var markedNode in GetNodesWithId( syntaxTree ) )
                {
                    var mark = GetNodeId( markedNode ).AssertNotNull();
                    var declaredSymbol = semanticModel.GetDeclaredSymbol( markedNode );
                    var referencedSymbol = semanticModel.GetSymbolInfo( markedNode ).Symbol;
                    var symbol = declaredSymbol ?? referencedSymbol;
                    nodeIdToSyntaxNode[mark] = markedNode;

                    if ( symbol == null )
                    {
                        continue;
                    }

                    syntaxNodeToSymbol[markedNode] = symbol;
                    nodeIdToCodeElement[mark] = symbolToCodeElement[symbol];
                }
            }

            var nameObliviousSignatureComparer =
                new StructuralSymbolComparer(
                    StructuralSymbolComparerOptions.GenericArguments
                    | StructuralSymbolComparerOptions.GenericParameterCount
                    | StructuralSymbolComparerOptions.ParameterModifiers
                    | StructuralSymbolComparerOptions.ParameterTypes );

            // Update transformations to reflect the input compilation.
            foreach ( var transformation in rewriter.ObservableTransformations.Cast<object>().Concat( rewriter.NonObservableTransformations ) )
            {
                var containingNodeId = ((ITestTransformation) transformation).ContainingNodeId;
                var insertPositionNodeId = ((ITestTransformation) transformation).InsertPositionNodeId;
                var insertPositionRelation = ((ITestTransformation) transformation).InsertPositionRelation;
                var symbolHelperNodeId = ((ITestTransformation) transformation).SymbolHelperNodeId;

                if ( transformation is IOverriddenDeclaration overriddenDeclaration )
                {
                    var overriddenDeclarationName = ((ITestTransformation) transformation).OverriddenDeclarationName;

                    var containingNode = nodeIdToSyntaxNode[containingNodeId];
                    var insertPositionNode = nodeIdToSyntaxNode[insertPositionNodeId];
                    var symbolHelperNode = nodeIdToSyntaxNode[symbolHelperNodeId];

                    var containingSymbol = (ITypeSymbol) syntaxNodeToSymbol[containingNode];
                    var symbolHelperSymbol = syntaxNodeToSymbol[symbolHelperNode];

                    var overridenMemberSymbol = containingSymbol.GetMembers()
                        .Where( x => StringComparer.Ordinal.Equals( x.Name, overriddenDeclarationName ) )
                        .Where( x => nameObliviousSignatureComparer.Equals( x, symbolHelperSymbol ) )
                        .Single();

                    var overridenMember = symbolToCodeElement[overridenMemberSymbol];

                    A.CallTo( () => ((IMemberIntroduction) overriddenDeclaration).InsertPosition )
                        .Returns( new InsertPosition( insertPositionRelation, (MemberDeclarationSyntax) insertPositionNode ) );

                    A.CallTo( () => overriddenDeclaration.OverriddenDeclaration ).Returns( overridenMember );
                    A.CallTo( () => ((IMemberIntroduction) overriddenDeclaration).TargetSyntaxTree ).Returns( symbolHelperNode.SyntaxTree );
                }
                else if ( transformation is IObservableTransformation observableTransformation )
                {
                    var introducedElementName = ((ITestTransformation) transformation).IntroducedElementName;

                    var symbolHelperNode = nodeIdToSyntaxNode[symbolHelperNodeId];
                    var insertPositionNode = nodeIdToSyntaxNode[insertPositionNodeId];

                    var containingDeclaration = nodeIdToCodeElement[containingNodeId];
                    var symbolHelperElement = (IMethod) nodeIdToCodeElement[symbolHelperNodeId];

                    A.CallTo( () => observableTransformation.ContainingDeclaration ).Returns( containingDeclaration );

                    A.CallTo( () => ((IMemberIntroduction) observableTransformation).InsertPosition )
                        .Returns( new InsertPosition( insertPositionRelation, (MemberDeclarationSyntax) insertPositionNode ) );

                    A.CallTo( () => ((IMemberIntroduction) observableTransformation).TargetSyntaxTree ).Returns( symbolHelperNode.SyntaxTree );

                    // ReSharper disable SuspiciousTypeConversion.Global

                    // TODO: This should be a deep copy of declarations to have a correct parent.
                    A.CallTo( () => ((IMethod) observableTransformation).LocalFunctions ).Returns( symbolHelperElement.LocalFunctions );
                    A.CallTo( () => ((IMethod) observableTransformation).Parameters ).Returns( symbolHelperElement.Parameters );
                    A.CallTo( () => ((IMethod) observableTransformation).GenericParameters ).Returns( symbolHelperElement.GenericParameters );
                    A.CallTo( () => ((IMethod) observableTransformation).GenericArguments ).Returns( symbolHelperElement.GenericArguments );
                    A.CallTo( () => ((IMethod) observableTransformation).ReturnParameter ).Returns( symbolHelperElement.ReturnParameter );
                    A.CallTo( () => ((IMethod) observableTransformation).ReturnType ).Returns( symbolHelperElement.ReturnType );
                    A.CallTo( () => ((IMethod) observableTransformation).Attributes ).Returns( symbolHelperElement.Attributes );
                    A.CallTo( () => ((IMethod) observableTransformation).Accessibility ).Returns( symbolHelperElement.Accessibility );
                    A.CallTo( () => ((IMethod) observableTransformation).Compilation ).Returns( symbolHelperElement.Compilation );
                    A.CallTo( () => ((IMethod) observableTransformation).DeclaringType ).Returns( symbolHelperElement.DeclaringType );
                    A.CallTo( () => ((IMethod) observableTransformation).DeclarationKind ).Returns( symbolHelperElement.DeclarationKind );
                    A.CallTo( () => ((IMethod) observableTransformation).IsAbstract ).Returns( symbolHelperElement.IsAbstract );
                    A.CallTo( () => ((IMethod) observableTransformation).IsAsync ).Returns( symbolHelperElement.IsAsync );
                    A.CallTo( () => ((IMethod) observableTransformation).IsNew ).Returns( symbolHelperElement.IsNew );
                    A.CallTo( () => ((IMethod) observableTransformation).IsOpenGeneric ).Returns( symbolHelperElement.IsOpenGeneric );
                    A.CallTo( () => ((IMethod) observableTransformation).IsOverride ).Returns( symbolHelperElement.IsOverride );
                    A.CallTo( () => ((IMethod) observableTransformation).IsReadOnly ).Returns( symbolHelperElement.IsReadOnly );
                    A.CallTo( () => ((IMethod) observableTransformation).IsSealed ).Returns( symbolHelperElement.IsSealed );
                    A.CallTo( () => ((IMethod) observableTransformation).IsStatic ).Returns( symbolHelperElement.IsStatic );
                    A.CallTo( () => ((IMethod) observableTransformation).IsVirtual ).Returns( symbolHelperElement.IsVirtual );
                    A.CallTo( () => ((IMethod) observableTransformation).MethodKind ).Returns( symbolHelperElement.MethodKind );
                    A.CallTo( () => ((IMethod) observableTransformation).Name ).Returns( introducedElementName.AssertNotNull() );
                }
            }
        }
    }
}