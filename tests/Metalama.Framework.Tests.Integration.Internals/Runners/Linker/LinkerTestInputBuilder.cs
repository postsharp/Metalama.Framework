﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using FakeItEasy;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Linking;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities.Comparers;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.TestFramework;
using Metalama.TestFramework.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Metalama.Framework.Tests.Integration.Runners.Linker
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
        private readonly IServiceProvider _serviceProvider;
        private readonly TestRewriter _rewriter;

        public LinkerTestInputBuilder( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
            this._rewriter = new TestRewriter( serviceProvider );
        }

        internal SyntaxNode ProcessSyntaxRoot( SyntaxNode syntaxRoot )
        {
            return this._rewriter.Visit( syntaxRoot )!;
        }

        public AspectLinkerInput ToAspectLinkerInput( PartialCompilation inputCompilation )
        {
            var initialCompilationModel = CompilationModel.CreateInitialInstance( new NullProject( this._serviceProvider ), inputCompilation );

            FinalizeTransformationFakes( this._rewriter, (CSharpCompilation) inputCompilation.Compilation, initialCompilationModel );

            var orderedLayers = this._rewriter.OrderedAspectLayers
                .Select( ( al, i ) => new OrderedAspectLayer( i, al.AspectName.AssertNotNull(), al.LayerName ) )
                .ToArray();

            var layerOrderLookup = orderedLayers.ToDictionary( x => x.AspectLayerId, x => x.Order );

            // TODO: All transformations should be ordered together, but there are no tests that would require that.
            var replacedCompilationModel = initialCompilationModel.WithTransformations(
                this._rewriter.ReplacedTransformations.OrderBy( x => layerOrderLookup[x.ParentAdvice.AspectLayerId] ).ToList() );

            var inputCompilationModel = replacedCompilationModel.WithTransformations(
                this._rewriter.ObservableTransformations.OrderBy( x => layerOrderLookup[x.ParentAdvice.AspectLayerId] ).ToList() );

            var linkerInput = new AspectLinkerInput(
                inputCompilation,
                inputCompilationModel,
                this._rewriter.ReplacedTransformations.Cast<ITransformation>()
                    .Concat( this._rewriter.ObservableTransformations )
                    .Concat( this._rewriter.NonObservableTransformations )
                    .OrderBy( x => layerOrderLookup[x.ParentAdvice.AspectLayerId] )
                    .ToList(),
                orderedLayers,
                new ArraySegment<ScopedSuppression>( Array.Empty<ScopedSuppression>() ),
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

                    var declaringNode = markedNode switch
                    {
                        FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Variables.Single(),
                        EventFieldDeclarationSyntax eventFieldDeclaration => eventFieldDeclaration.Declaration.Variables.Single(),
                        _ => markedNode
                    };

                    var declaredSymbol = semanticModel.GetDeclaredSymbol( declaringNode );
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
            foreach ( var transformation in rewriter.ObservableTransformations.Cast<object>()
                         .Concat( rewriter.NonObservableTransformations )
                         .Concat( rewriter.ReplacedTransformations ) )
            {
                var containingNodeId = ((ITestTransformation) transformation).ContainingNodeId;
                var insertPositionNodeId = ((ITestTransformation) transformation).InsertPositionNodeId;
                var insertPositionRelation = ((ITestTransformation) transformation).InsertPositionRelation;
                var symbolHelperNodeId = ((ITestTransformation) transformation).SymbolHelperNodeId;
                var symbolHelperNode = nodeIdToSyntaxNode[symbolHelperNodeId];
                var symbolHelperSymbol = syntaxNodeToSymbol[symbolHelperNode];
                var containingNode = nodeIdToSyntaxNode[containingNodeId];
                var containingSymbol = (ITypeSymbol) syntaxNodeToSymbol[containingNode];

                if ( transformation is IOverriddenDeclaration overriddenDeclaration )
                {
                    var overriddenDeclarationName = ((ITestTransformation) transformation).OverriddenDeclarationName.AssertNotNull();

                    var insertPositionNode =
                        insertPositionNodeId != null
                            ? nodeIdToSyntaxNode[insertPositionNodeId]
                            : containingNode;

                    if ( insertPositionNode is VariableDeclaratorSyntax )
                    {
                        insertPositionNode = insertPositionNode.Parent?.Parent.AssertNotNull();
                    }

                    var overriddenMemberSymbol = containingSymbol.GetMembers()
                        .Where(
                            x =>
                                StringComparer.Ordinal.Equals( x.Name, overriddenDeclarationName )
                                || (overriddenDeclarationName.ContainsOrdinal( '.' ) && x.Name.EndsWith(
                                    overriddenDeclarationName,
                                    StringComparison.Ordinal )) )
                        .Where( x => nameObliviousSignatureComparer.Equals( x, symbolHelperSymbol ) )
                        .SingleOrDefault();

                    IDeclaration? overridenMember;

                    if ( overriddenMemberSymbol != null )
                    {
                        overridenMember = symbolToCodeElement[overriddenMemberSymbol];
                    }
                    else
                    {
                        // Find introduction's symbol helper.
                        var overriddenMemberSymbolHelper = containingSymbol.GetMembers()
                            .Where( x => StringComparer.Ordinal.Equals( x.Name, GetSymbolHelperName( overriddenDeclarationName ) ) )
                            .Where( x => nameObliviousSignatureComparer.Equals( x, symbolHelperSymbol ) )
                            .SingleOrDefault();

                        // Find the transformation for this symbol helper.
                        var overriddenMemberSymbolHelperNodeId =
                            GetNodeId( overriddenMemberSymbolHelper.AssertNotNull().GetPrimaryDeclaration().AssertNotNull() );

                        overridenMember = (IDeclaration) rewriter.ObservableTransformations
                            .Where( t => ((ITestTransformation) t).SymbolHelperNodeId == overriddenMemberSymbolHelperNodeId )
                            .Single();
                    }

                    if ( insertPositionNode != null )
                    {
                        A.CallTo( () => ((IIntroduceMemberTransformation) overriddenDeclaration).InsertPosition )
                            .Returns( new InsertPosition( insertPositionRelation, (MemberDeclarationSyntax) insertPositionNode ) );
                    }
                    else
                    {
                        throw new AssertionFailedException();
                    }

                    A.CallTo( () => overriddenDeclaration.OverriddenDeclaration ).Returns( overridenMember );

                    A.CallTo( () => ((IIntroduceMemberTransformation) overriddenDeclaration).TransformedSyntaxTree )
                        .Returns( symbolHelperNode.SyntaxTree );
                }
                else if ( transformation is IObservableTransformation observableTransformation )
                {
                    var introducedElementName = ((ITestTransformation) transformation).IntroducedElementName.AssertNotNull();

                    A.CallTo( () => ((IDeclarationImpl) observableTransformation).Compilation ).Returns( initialCompilationModel );

                    var insertPositionNode =
                        insertPositionNodeId != null
                            ? nodeIdToSyntaxNode[insertPositionNodeId]
                            : containingNode;

                    if ( insertPositionNode is VariableDeclaratorSyntax )
                    {
                        insertPositionNode = insertPositionNode.Parent?.Parent.AssertNotNull();
                    }

                    if ( transformation is IReplaceMemberTransformation replaceMember )
                    {
                        // This only supports fields being replaced by properties.
                        var replacedElementName = ((ITestTransformation) transformation).ReplacedElementName;

                        // Find symbol helper for the replaced source declaration.
                        var replacedMemberSymbol = containingSymbol.GetMembers()
                            .SingleOrDefault( x => StringComparer.Ordinal.Equals( x.Name, replacedElementName ) );

                        if ( replacedMemberSymbol != null )
                        {
                            // This is replaced source element.
                            A.CallTo( () => replaceMember.ReplacedMember )
                                .Returns( new MemberRef<IMember>( replacedMemberSymbol, inputCompilation ) );
                        }
                        else
                        {
                            // This is replaced builder.
                            var replacedMemberSymbolHelperSymbol = containingSymbol.GetMembers()
                                .Single( x => StringComparer.Ordinal.Equals( x.Name, GetSymbolHelperName( GetReplacedMemberName( replacedElementName ) ) ) );

                            var replacedMemberSymbolHelperNode = replacedMemberSymbolHelperSymbol switch
                            {
                                IFieldSymbol => replacedMemberSymbolHelperSymbol.GetPrimaryDeclaration()?.Parent?.Parent,
                                _ => replacedMemberSymbolHelperSymbol.GetPrimaryDeclaration()
                            };

                            var replacedSymbolHelperNodeId = GetNodeId( replacedMemberSymbolHelperNode.AssertNotNull() );

                            var replacedTransformation =
                                rewriter.ReplacedTransformations.Single( x => ((ITestTransformation) x).SymbolHelperNodeId == replacedSymbolHelperNodeId );

                            A.CallTo( () => ((IDeclarationImpl) replacedTransformation).Compilation ).Returns( initialCompilationModel );

                            A.CallTo( () => replaceMember.ReplacedMember )
                                .Returns( new MemberRef<IMember>( (IMemberOrNamedTypeBuilder) replacedTransformation ) );
                        }
                    }

                    var containingDeclaration = nodeIdToCodeElement[containingNodeId];

                    if ( nodeIdToCodeElement.ContainsKey( symbolHelperNodeId ) )
                    {
                        switch ( nodeIdToCodeElement[symbolHelperNodeId] )
                        {
                            case IMethod symbolHelperMethod:
                                FinalizeTransformationMethod(
                                    observableTransformation,
                                    symbolHelperNode,
                                    symbolHelperMethod,
                                    containingDeclaration,
                                    insertPositionRelation,
                                    insertPositionNode!,
                                    introducedElementName );

                                break;

                            case IProperty symbolHelperProperty:
                                FinalizeTransformationProperty(
                                    observableTransformation,
                                    symbolHelperNode,
                                    symbolHelperProperty,
                                    containingDeclaration,
                                    insertPositionRelation,
                                    insertPositionNode!,
                                    introducedElementName );

                                break;

                            case IEvent symbolHelperEvent:
                                FinalizeTransformationEvent(
                                    observableTransformation,
                                    symbolHelperNode,
                                    symbolHelperEvent,
                                    containingDeclaration,
                                    insertPositionRelation,
                                    insertPositionNode!,
                                    introducedElementName );

                                break;

                            case IField symbolHelperField:
                                FinalizeTransformationField(
                                    observableTransformation,
                                    symbolHelperNode,
                                    symbolHelperField,
                                    containingDeclaration,
                                    insertPositionRelation,
                                    insertPositionNode!,
                                    introducedElementName );

                                break;

                            default:
                                throw new AssertionFailedException();
                        }
                    }
                }
            }
        }

        private static void FinalizeTransformationMethod(
            IObservableTransformation observableTransformation,
            SyntaxNode symbolHelperNode,
            IMethod symbolHelperElement,
            IDeclaration containingDeclaration,
            InsertPositionRelation insertPositionRelation,
            SyntaxNode insertPositionNode,
            string introducedElementName )
        {
            FinalizeTransformation(
                observableTransformation,
                symbolHelperNode,
                symbolHelperElement,
                containingDeclaration,
                insertPositionRelation,
                insertPositionNode,
                introducedElementName );

            A.CallTo( () => ((IMethod) observableTransformation).Parameters ).Returns( symbolHelperElement.Parameters );
            A.CallTo( () => ((IMethod) observableTransformation).TypeParameters ).Returns( symbolHelperElement.TypeParameters );
            A.CallTo( () => ((IMethod) observableTransformation).ReturnParameter ).Returns( symbolHelperElement.ReturnParameter );
            A.CallTo( () => ((IMethod) observableTransformation).ReturnType ).Returns( symbolHelperElement.ReturnType );
            A.CallTo( () => ((IMethod) observableTransformation).IsOpenGeneric ).Returns( symbolHelperElement.IsOpenGeneric );
            A.CallTo( () => ((IMethod) observableTransformation).IsReadOnly ).Returns( symbolHelperElement.IsReadOnly );
            A.CallTo( () => ((IMethod) observableTransformation).MethodKind ).Returns( symbolHelperElement.MethodKind );
        }

        private static void FinalizeTransformationProperty(
            IObservableTransformation observableTransformation,
            SyntaxNode symbolHelperNode,
            IProperty symbolHelperElement,
            IDeclaration containingDeclaration,
            InsertPositionRelation insertPositionRelation,
            SyntaxNode insertPositionNode,
            string introducedElementName )
        {
            FinalizeTransformation(
                observableTransformation,
                symbolHelperNode,
                symbolHelperElement,
                containingDeclaration,
                insertPositionRelation,
                insertPositionNode,
                introducedElementName );

            A.CallTo( () => ((IProperty) observableTransformation).Type ).Returns( symbolHelperElement.Type );
        }

        private static void FinalizeTransformationField(
            IObservableTransformation observableTransformation,
            SyntaxNode symbolHelperNode,
            IField symbolHelperField,
            IDeclaration containingDeclaration,
            InsertPositionRelation insertPositionRelation,
            SyntaxNode insertPositionNode,
            string introducedElementName )
        {
            FinalizeTransformation(
                observableTransformation,
                symbolHelperNode,
                symbolHelperField,
                containingDeclaration,
                insertPositionRelation,
                insertPositionNode,
                introducedElementName );

            A.CallTo( () => ((IField) observableTransformation).Type ).Returns( symbolHelperField.Type );
        }

        private static void FinalizeTransformationEvent(
            IObservableTransformation observableTransformation,
            SyntaxNode symbolHelperNode,
            IEvent symbolHelperElement,
            IDeclaration containingDeclaration,
            InsertPositionRelation insertPositionRelation,
            SyntaxNode insertPositionNode,
            string introducedElementName )
        {
            FinalizeTransformation(
                observableTransformation,
                symbolHelperNode,
                symbolHelperElement,
                containingDeclaration,
                insertPositionRelation,
                insertPositionNode,
                introducedElementName );

            A.CallTo( () => ((IEvent) observableTransformation).Type ).Returns( symbolHelperElement.Type );
        }

        private static void FinalizeTransformation(
            IObservableTransformation observableTransformation,
            SyntaxNode symbolHelperNode,
            IMember symbolHelperElement,
            IDeclaration containingDeclaration,
            InsertPositionRelation insertPositionRelation,
            SyntaxNode insertPositionNode,
            string introducedElementName )
        {
            A.CallTo( () => observableTransformation.ContainingDeclaration ).Returns( containingDeclaration );

            A.CallTo( () => ((IDeclarationImpl) observableTransformation).ToRef() )
                .Returns( new Ref<IDeclaration>( (IDeclarationBuilder) observableTransformation ) );

            A.CallTo( () => ((IIntroduceMemberTransformation) observableTransformation).InsertPosition )
                .Returns( new InsertPosition( insertPositionRelation, (MemberDeclarationSyntax) insertPositionNode ) );

            A.CallTo( () => ((IIntroduceMemberTransformation) observableTransformation).TransformedSyntaxTree )
                .Returns( symbolHelperNode.SyntaxTree );

            // ReSharper disable SuspiciousTypeConversion.Global

            // TODO: This should be a deep copy of declarations to have a correct parent.
            A.CallTo( () => ((IMember) observableTransformation).ContainingDeclaration ).Returns( containingDeclaration );
            A.CallTo( () => ((IMember) observableTransformation).Attributes ).Returns( symbolHelperElement.Attributes );
            A.CallTo( () => ((IMember) observableTransformation).Accessibility ).Returns( symbolHelperElement.Accessibility );
            A.CallTo( () => ((IMember) observableTransformation).Compilation ).Returns( symbolHelperElement.Compilation );
            A.CallTo( () => ((IMember) observableTransformation).DeclaringType ).Returns( symbolHelperElement.DeclaringType );
            A.CallTo( () => ((IMember) observableTransformation).DeclarationKind ).Returns( symbolHelperElement.DeclarationKind );
            A.CallTo( () => ((IMember) observableTransformation).IsAbstract ).Returns( symbolHelperElement.IsAbstract );
            A.CallTo( () => ((IMember) observableTransformation).IsAsync ).Returns( symbolHelperElement.IsAsync );
            A.CallTo( () => ((IMember) observableTransformation).IsNew ).Returns( symbolHelperElement.IsNew );
            A.CallTo( () => ((IMember) observableTransformation).IsOverride ).Returns( symbolHelperElement.IsOverride );
            A.CallTo( () => ((IMember) observableTransformation).IsSealed ).Returns( symbolHelperElement.IsSealed );
            A.CallTo( () => ((IMember) observableTransformation).IsStatic ).Returns( symbolHelperElement.IsStatic );
            A.CallTo( () => ((IMember) observableTransformation).IsVirtual ).Returns( symbolHelperElement.IsVirtual );
            A.CallTo( () => ((IMember) observableTransformation).Name ).Returns( introducedElementName.AssertNotNull() );
        }
    }
}