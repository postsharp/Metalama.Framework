﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    // Lexical scopes and template expansion:
    // ----------------------------------------
    // When call graph of overrides of a single method is simple enough, we can inline calls to produce nicer code.
    // Names in these methods can, however, collide if we don't take care.
    //
    // Template expansion uses lexical scopes to avoid name collision with initial scope and within the expanded syntax.
    // We can use this mechanism to avoid name collision immediately during template expansion.
    //
    // Consider the following example of lexical scope trees for a original method and two overrides:
    // 
    // Original method:      Template method 1:      Template method 2:
    //                                               
    // R1----O               R2----O                 R3----O 
    //  \                     \                       \
    //   ----O-----O           ----O-----P2            ----O-----P3
    //        \                     \                       \
    //         ----O                 ----O                   ----O
    //
    // Final inlined lexical scope tree (our goal):
    // 
    // R3'---O 
    //  \
    //   ----O-------R2'---O       
    //        \       \            
    //         ----O   ----O-------R1'---O 
    //                      \       \
    //                       ----O   ----O-----O
    //                                    \
    //                                     ----O
    //
    // We want to avoid rewriting names in the linked tree. To have correct syntax tree in this case, we need the following to hold: 
    //   * Names(Subtree(R1)) ⋂ Names(Path(R3', R1')) = Ø
    //   * Names(Subtree(R2)) ⋂ Names(Path(R3', R1')) = Ø
    //
    // Property of template expansion: Names(Subtree(Expanded)) ⋂ Names(Subtree(Input)) = Ø
    //
    // We (for now) do a suboptimal solution by feeding the template expansion the following:
    //   * Template 1: Names(Subtree(R1))
    //   * Template 2: Names(Subtree(R1)) ⋃ Names(Subtree(R2))
    //
    // Therefore:
    //   * Names(Subtree(R3')) = Names(Subtree(R1)) ⋃ Names(Subtree(R2)) ⋃ Names(Subtree(R3))
    //   * Names(Subtree(R1)), Names(Subtree(R2)), Names(Subtree(R3)) are mutually disjoint

    /// <summary>
    /// Aspect linker introduction steps. Adds introduced members from all transformation to the Roslyn compilation. This involves calling template expansion.
    /// This results in the transformation registry and intermediate compilation, and also produces diagnostics.
    /// </summary>
    internal partial class LinkerIntroductionStep : AspectLinkerPipelineStep<AspectLinkerInput, LinkerIntroductionStepOutput>
    {
        private readonly IServiceProvider _serviceProvider;

        public LinkerIntroductionStep( IServiceProvider serviceProvider )
        {
            serviceProvider.GetRequiredService<ServiceProviderMark>().RequireProjectWide();

            this._serviceProvider = serviceProvider;
        }

        public override LinkerIntroductionStepOutput Execute( AspectLinkerInput input )
        {
            // We don't use a code fix filter because the linker is not supposed to suggest code fixes. If that changes, we need to pass a filter.
            var diagnostics = new UserDiagnosticSink( input.CompileTimeProject, null );

            var nameProvider = new LinkerIntroductionNameProvider();
            var syntaxTransformationCollection = new SyntaxTransformationCollection();

            // TODO: Merge observable and non-observable transformations so that the order is preserved.
            //       Maybe have all transformations already together in the input?
            var allTransformations =
                MergeOrderedTransformations(
                        input.OrderedAspectLayers,
                        input.CompilationModel.GetAllObservableTransformations( false ).Select( x => x.Transformations.OfType<ISyntaxTreeTransformation>() ),
                        input.NonObservableTransformations.OfType<ISyntaxTreeTransformation>() )
                    .ToList();

            ProcessReplaceTransformations( input, allTransformations, syntaxTransformationCollection, out var replacedTransformations );
            ProcessHierarchicalTransformations( diagnostics, allTransformations, nameProvider, out var initializationResults );
            this.ProcessIntroduceTransformations( input, allTransformations, diagnostics, nameProvider, syntaxTransformationCollection, replacedTransformations, initializationResults );
            PrepareCodeTransformationMarkedNodes( input, allTransformations, initializationResults, out var markedNodes, out var typesWithRequiredImplicitCtors );

            // Group diagnostic suppressions by target.
            var suppressionsByTarget = input.DiagnosticSuppressions.ToMultiValueDictionary(
                s => s.Declaration,
                input.CompilationModel.InvariantComparer );

            Rewriter rewriter = new(
                syntaxTransformationCollection,
                suppressionsByTarget,
                input.CompilationModel,
                input.OrderedAspectLayers,
                markedNodes,
                typesWithRequiredImplicitCtors );

            var syntaxTreeMapping = new Dictionary<SyntaxTree, SyntaxTree>();

            // Process syntax trees one by one.
            var intermediateCompilation = input.InitialCompilation;

            foreach ( var initialSyntaxTree in input.InitialCompilation.SyntaxTrees.Values )
            {
                var oldRoot = initialSyntaxTree.GetRoot();
                var newRoot = rewriter.Visit( oldRoot ).AssertNotNull();

                if ( oldRoot != newRoot )
                {
                    var intermediateSyntaxTree = initialSyntaxTree.WithRootAndOptions( newRoot, initialSyntaxTree.Options );

                    syntaxTreeMapping.Add( initialSyntaxTree, intermediateSyntaxTree );
                }
            }

            var codeTransformationRegistry = new LinkerCodeTransformationRegistry( input.CompilationModel, markedNodes.ToDictionary( x => x.Value.Id, x => x.Value.Marks ) );

            intermediateCompilation = intermediateCompilation.Update(
                syntaxTreeMapping.Select( p => new SyntaxTreeModification( p.Value, p.Key ) ).ToList(),
                Array.Empty<SyntaxTree>() );

            var introductionRegistry = new LinkerIntroductionRegistry(
                input.CompilationModel,
                intermediateCompilation.Compilation,
                syntaxTreeMapping,
                syntaxTransformationCollection.IntroducedMembers );

            var projectOptions = this._serviceProvider.GetService<IProjectOptions>();

            return new LinkerIntroductionStepOutput(
                diagnostics,
                input.CompilationModel,
                intermediateCompilation,
                introductionRegistry,
                codeTransformationRegistry,
                input.OrderedAspectLayers,
                projectOptions );
        }

        private static void ProcessReplaceTransformations(
            AspectLinkerInput input,
            List<ITransformation> allTransformations,
            SyntaxTransformationCollection syntaxTransformationCollection,
            out HashSet<ISyntaxTreeTransformation> replacedTransformations )
        {
            replacedTransformations = new HashSet<ISyntaxTreeTransformation>();

            foreach ( var transformation in allTransformations.OfType<IReplaceMember>() )
            {
                var replacedMember = transformation.ReplacedMember.GetTarget( input.CompilationModel );

                IDeclaration canonicalReplacedMember = replacedMember switch
                {
                    BuiltDeclaration declaration => declaration.Builder,
                    _ => replacedMember
                };

                switch ( canonicalReplacedMember )
                {
                    case Field replacedField:
                        var syntaxReference = replacedField.Symbol.GetPrimarySyntaxReference();

                        if ( syntaxReference == null )
                        {
                            throw new AssertionFailedException();
                        }

                        var removedSyntax = syntaxReference.GetSyntax();

                        syntaxTransformationCollection.AddRemovedSyntax( removedSyntax );

                        break;

                    case ISyntaxTreeTransformation replacedTransformation:
                        replacedTransformations.Add( replacedTransformation );

                        break;

                    default:
                        throw new AssertionFailedException();
                }
            }
        }

        private static void ProcessHierarchicalTransformations(
            UserDiagnosticSink diagnostics,
            List<ITransformation> allTransformations,
            LinkerIntroductionNameProvider nameProvider,
            out Dictionary<IHierarchicalTransformation, TransformationInitializationResult?> initializationResults )
        {
            initializationResults = new Dictionary<IHierarchicalTransformation, TransformationInitializationResult?>();
            var initializationContext = new InitializationContext( diagnostics, nameProvider, initializationResults );

            // Initialize hierarchical transformations.
            foreach ( var hierarchicalTransformation in allTransformations.OfType<IHierarchicalTransformation>().OrderByReverseTopology( t => t.Dependencies ) )
            {
                initializationResults[hierarchicalTransformation] = hierarchicalTransformation.Initialize( initializationContext );
            }
        }

        private void ProcessIntroduceTransformations(
            AspectLinkerInput input,
            List<ITransformation> allTransformations,
            UserDiagnosticSink diagnostics,
            LinkerIntroductionNameProvider nameProvider,
            SyntaxTransformationCollection syntaxTransformationCollection,
            HashSet<ISyntaxTreeTransformation>? replacedTransformations,
            Dictionary<IHierarchicalTransformation, TransformationInitializationResult?> initializationResults )
        {
            var lexicalScopeFactory = new LexicalScopeFactory( input.CompilationModel );

            // Visit all transformations, respect aspect part ordering.
            foreach ( var transformation in allTransformations )
            {
                if ( replacedTransformations.Contains( transformation ) )
                {
                    continue;
                }

                if ( transformation is IMemberIntroduction memberIntroduction )
                {
                    // Create the SyntaxGenerationContext for the insertion point.
                    var positionInSyntaxTree = 0;

                    if ( memberIntroduction.InsertPosition.SyntaxNode != null )
                    {
                        switch ( memberIntroduction.InsertPosition.Relation )
                        {
                            case InsertPositionRelation.After:
                                positionInSyntaxTree = memberIntroduction.InsertPosition.SyntaxNode.Span.End + 1;

                                break;

                            case InsertPositionRelation.Within:
                                positionInSyntaxTree = ((BaseTypeDeclarationSyntax) memberIntroduction.InsertPosition.SyntaxNode).CloseBraceToken.Span.Start
                                                       - 1;

                                break;

                            default:
                                positionInSyntaxTree = 0;

                                break;
                        }
                    }

                    var syntaxGenerationContext = SyntaxGenerationContext.Create(
                        this._serviceProvider,
                        input.InitialCompilation.Compilation,
                        memberIntroduction.TargetSyntaxTree,
                        positionInSyntaxTree );

                    var initializationResult =
                        transformation is IHierarchicalTransformation hierarchicalTransformation
                        ? initializationResults[hierarchicalTransformation]
                        : null;

                    // Call GetIntroducedMembers
                    var introductionContext = new MemberIntroductionContext(
                        diagnostics,
                        nameProvider,
                        lexicalScopeFactory,
                        syntaxGenerationContext,
                        this._serviceProvider,
                        initializationResult,
                        initializationResults );

                    var introducedMembers = memberIntroduction.GetIntroducedMembers( introductionContext );

                    syntaxTransformationCollection.Add( memberIntroduction, introducedMembers );
                }

                if ( transformation is IIntroducedInterface interfaceIntroduction )
                {
                    var introducedInterface = interfaceIntroduction.GetSyntax();
                    syntaxTransformationCollection.Add( interfaceIntroduction, introducedInterface );
                }
            }
        }

        private static void PrepareCodeTransformationMarkedNodes( 
            AspectLinkerInput input,
            List<ITransformation> allTransformations,
            Dictionary<IHierarchicalTransformation, TransformationInitializationResult?> initializationResults,
            out Dictionary<SyntaxNode, (string Id, IReadOnlyList<CodeTransformationMark> Marks)> markedNodes, 
            out Dictionary<ISymbol, (ConstructorDeclarationSyntax? Static, ConstructorDeclarationSyntax? Instance)> typesWithRequiredImplicitCtors )
        {
            var codeTransformationsBySyntaxTree = new Dictionary<SyntaxTree, List<ICodeTransformation>>();

            foreach ( var codeTransformationSource in allTransformations.OfType<ICodeTransformationSource>() )
            {
                var initializationResult =
                    codeTransformationSource is IHierarchicalTransformation hierarchicalTransformation
                    ? initializationResults[hierarchicalTransformation]
                    : null;

                var codeTransformationSourceContext =
                    new CodeTransformationSourceContext(
                        initializationResult,
                        initializationResults );

                var codeTransformations = codeTransformationSource.GetCodeTransformations( codeTransformationSourceContext );

                if ( !codeTransformationsBySyntaxTree.TryGetValue( codeTransformationSource.TargetSyntaxTree, out var list ) )
                {
                    codeTransformationsBySyntaxTree[codeTransformationSource.TargetSyntaxTree] = list = new List<ICodeTransformation>();
                }

                list.AddRange( codeTransformations );
            }

            markedNodes = new Dictionary<SyntaxNode, (string Id, IReadOnlyList<CodeTransformationMark> Marks)>();
            typesWithRequiredImplicitCtors = new Dictionary<ISymbol, (ConstructorDeclarationSyntax? Static, ConstructorDeclarationSyntax? Instance)>( SymbolEqualityComparer.Default );
            var nextMarkedNodeId = 0;

            // Collect transformation marks.
            // TODO: This does not work with introduced code (because it's not expanded yet).
            foreach ( var initialSyntaxTree in input.InitialCompilation.SyntaxTrees.Values )
            {
                var root = initialSyntaxTree.GetRoot();

                if ( codeTransformationsBySyntaxTree.TryGetValue( initialSyntaxTree, out var codeTransformations ) )
                {
                    var codeTransformationVisitor = new CodeTransformationVisitor( input.InitialCompilation.Compilation.GetSemanticModel( initialSyntaxTree ), codeTransformations );
                    codeTransformationVisitor.Visit( root );

                    // Temporary (implicit ctors).
                    foreach ( var type in codeTransformationVisitor.TypesWithRequiredImplicitCtors )
                    {
                        ConstructorDeclarationSyntax? staticCtor = null;
                        ConstructorDeclarationSyntax? instanceCtor = null;
                        if ( type.Value.Static )
                        {
                            var mark = nextMarkedNodeId++.ToString( CultureInfo.InvariantCulture );

                            staticCtor =
                                SyntaxFactory.ConstructorDeclaration(
                                    SyntaxFactory.List<AttributeListSyntax>(),
                                    SyntaxFactory.TokenList( SyntaxFactory.Token( SyntaxKind.StaticKeyword ) ),
                                    type.Value.Declaration.Identifier,
                                    SyntaxFactory.ParameterList(),
                                    null,
                                    SyntaxFactory.Block().WithLinkerMarkedNodeId( mark ),
                                    null )
                                .NormalizeWhitespace()
                                .WithTrailingTrivia( SyntaxFactory.ElasticLineFeed );
                        }

                        if ( !type.Value.Static )
                        {
                            var mark = nextMarkedNodeId++.ToString( CultureInfo.InvariantCulture );

                            instanceCtor =
                                SyntaxFactory.ConstructorDeclaration(
                                    SyntaxFactory.List<AttributeListSyntax>(),
                                    SyntaxFactory.TokenList( SyntaxFactory.Token( SyntaxKind.PublicKeyword ) ),
                                    type.Value.Declaration.Identifier,
                                    SyntaxFactory.ParameterList(),
                                    null,
                                    SyntaxFactory.Block().WithLinkerMarkedNodeId( mark ),
                                    null )
                                .NormalizeWhitespace()
                                .WithTrailingTrivia( SyntaxFactory.ElasticLineFeed );                                
                        }

                        typesWithRequiredImplicitCtors.Add(
                            type.Key,
                            (staticCtor, instanceCtor) );
                    }

                    if ( codeTransformationVisitor.Marks.Count > 0 )
                    {
                        foreach ( var mark in codeTransformationVisitor.Marks )
                        {
                            if ( mark.Target != null )
                            {
                                if ( !markedNodes.TryGetValue( mark.Target, out var record ) )
                                {
                                    markedNodes[mark.Target] = record = (nextMarkedNodeId++.ToString( CultureInfo.InvariantCulture ), new List<CodeTransformationMark>());
                                }

                                ((List<CodeTransformationMark>) record.Marks).Add( mark );
                            }
                            else
                            {
                                var declarationNode = mark.Source.TargetDeclaration.GetPrimaryDeclaration();

                                if ( declarationNode != null )
                                {
                                    if ( !markedNodes.TryGetValue( declarationNode, out var record ) )
                                    {
                                        markedNodes[declarationNode] = record = (nextMarkedNodeId++.ToString( CultureInfo.InvariantCulture ), new List<CodeTransformationMark>());
                                    }

                                    ((List<CodeTransformationMark>) record.Marks).Add( mark );
                                }
                                else
                                {
                                    // Temporary (implicit ctors).
                                    var typeSymbol = mark.Source.TargetDeclaration.ContainingDeclaration.AssertNotNull().GetSymbol().AssertNotNull();

                                    if ( typesWithRequiredImplicitCtors.TryGetValue( typeSymbol, out var typeRecord ) )
                                    {
                                        if (mark.Source.TargetDeclaration.IsStatic)
                                        {
                                            // Implicit static ctor.
                                            var key = typeRecord.Static.AssertNotNull();

                                            if ( !markedNodes.TryGetValue( key, out var record ) )
                                            {
                                                markedNodes[key] = record = (key.Body.AssertNotNull().GetLinkerMarkedNodeId().AssertNotNull(), new List<CodeTransformationMark>());
                                            }

                                            ((List<CodeTransformationMark>) record.Marks).Add( mark );
                                        }
                                        else
                                        {
                                            // Implicit instance ctor.
                                            var key = typeRecord.Instance.AssertNotNull();

                                            if ( !markedNodes.TryGetValue( key, out var record ) )
                                            {
                                                markedNodes[key] = record = (key.Body.AssertNotNull().GetLinkerMarkedNodeId().AssertNotNull(), new List<CodeTransformationMark>());
                                            }

                                            ((List<CodeTransformationMark>) record.Marks).Add( mark );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<ITransformation> MergeOrderedTransformations(
            IReadOnlyList<OrderedAspectLayer> orderedLayers,
            IEnumerable<IEnumerable<ITransformation>> observableTransformationLists,
            IEnumerable<ITransformation> nonObservableTransformations )
        {
            var enumerators = new LinkedList<IEnumerator<ITransformation>>();

            foreach ( var observableTransformations in observableTransformationLists )
            {
                enumerators.AddLast( observableTransformations.GetEnumerator() );
            }

            enumerators.AddLast( nonObservableTransformations.GetEnumerator() );

            // Initialize enumerators and remove empty ones.
            var currentEnumerator = enumerators.First;

            while ( currentEnumerator != null )
            {
                if ( !currentEnumerator.Value.MoveNext() )
                {
                    enumerators.Remove( currentEnumerator );
                }

                currentEnumerator = currentEnumerator.Next;
            }

            // Go through ordered layers and yield all transformations for these layers.
            // Presumes all input enumerable are ordered according to ordered layers.
            foreach ( var orderedLayer in orderedLayers )
            {
                currentEnumerator = enumerators.First;

                if ( currentEnumerator == null )
                {
                    break;
                }

                do
                {
                    var current = currentEnumerator.Value.Current.AssertNotNull();

                    while ( current.Advice.AspectLayerId == orderedLayer.AspectLayerId )
                    {
                        yield return current;

                        if ( !currentEnumerator.Value.MoveNext() )
                        {
                            var toRemove = currentEnumerator;
                            currentEnumerator = currentEnumerator.Next;
                            enumerators.Remove( toRemove );

                            goto next;
                        }

                        current = currentEnumerator.Value.Current;
                    }

                    currentEnumerator = currentEnumerator.Next;

                next:

                    // Comment to make the formatter happy.

                    ;
                }
                while ( currentEnumerator != null );
            }
        }
    }
}