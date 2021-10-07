// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.Linking
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
            this._serviceProvider = serviceProvider;
        }

        public override LinkerIntroductionStepOutput Execute( AspectLinkerInput input )
        {
            var diagnostics = new UserDiagnosticSink( input.CompileTimeProject );
            var nameProvider = new LinkerIntroductionNameProvider();
            var lexicalScopeFactory = new LexicalScopeFactory( input.CompilationModel );
            var syntaxTransformationCollection = new SyntaxTransformationCollection();

            // TODO: Merge observable and non-observable transformations so that the order is preserved.
            //       Maybe have all transformations already together in the input?
            var allTransformations =
                MergeOrderedTransformations(
                        input.OrderedAspectLayers,
                        input.CompilationModel.GetAllObservableTransformations().Select( x => x.Transformations.OfType<ISyntaxTreeTransformation>() ),
                        input.NonObservableTransformations.OfType<ISyntaxTreeTransformation>() )
                    .ToList();

            var replacedTransformations = new HashSet<ISyntaxTreeTransformation>();

            foreach ( var transformation in allTransformations.OfType<IReplaceMember>() )
            {
                var replacedMember = transformation.ReplacedMember.Resolve( input.CompilationModel );

                switch ( replacedMember )
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

                    // Call GetIntroducedMembers
                    var introductionContext = new MemberIntroductionContext(
                        diagnostics,
                        nameProvider,
                        lexicalScopeFactory,
                        syntaxGenerationContext,
                        this._serviceProvider );

                    var introducedMembers = memberIntroduction.GetIntroducedMembers( introductionContext );

                    syntaxTransformationCollection.Add( memberIntroduction, introducedMembers );
                }

                if ( transformation is IIntroducedInterface interfaceIntroduction )
                {
                    var introducedInterfaces = interfaceIntroduction.GetIntroducedInterfaceImplementations();
                    syntaxTransformationCollection.Add( interfaceIntroduction, introducedInterfaces );
                }
            }

            // Group diagnostic suppressions by target.
            var suppressionsByTarget = input.DiagnosticSuppressions.ToMultiValueDictionary(
                s => s.Declaration,
                input.CompilationModel.InvariantComparer );

            // Process syntax trees one by one.
            var intermediateCompilation = input.InitialCompilation;

            Rewriter addIntroducedElementsRewriter = new(
                syntaxTransformationCollection,
                suppressionsByTarget,
                input.CompilationModel,
                input.OrderedAspectLayers );

            var syntaxTreeMapping = new Dictionary<SyntaxTree, SyntaxTree>();

            foreach ( var initialSyntaxTree in input.InitialCompilation.SyntaxTrees.Values )
            {
                var oldRoot = initialSyntaxTree.GetRoot();
                var newRoot = addIntroducedElementsRewriter.Visit( oldRoot );

                if ( oldRoot != newRoot )
                {
                    var intermediateSyntaxTree = initialSyntaxTree.WithRootAndOptions( newRoot, initialSyntaxTree.Options );

                    syntaxTreeMapping.Add( initialSyntaxTree, intermediateSyntaxTree );
                }
            }

            intermediateCompilation = intermediateCompilation.Update(
                syntaxTreeMapping.Select( p => new SyntaxTreeModification( p.Value, p.Key ) ).ToList(),
                Array.Empty<SyntaxTree>() );

            var introductionRegistry = new LinkerIntroductionRegistry(
                input.CompilationModel,
                intermediateCompilation.Compilation,
                syntaxTreeMapping,
                syntaxTransformationCollection.IntroducedMembers );

            return new LinkerIntroductionStepOutput(
                diagnostics,
                input.CompilationModel,
                intermediateCompilation,
                introductionRegistry,
                input.OrderedAspectLayers );
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