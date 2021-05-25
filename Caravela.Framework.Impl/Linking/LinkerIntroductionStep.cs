// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;
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
            var lexicalScopeHelper = new LexicalScopeFactory( input.CompilationModel );
            var introducedCollection = new IntroductionCollection();
            var syntaxTreeMapping = new Dictionary<SyntaxTree, SyntaxTree>();
            var syntaxFactory = ReflectionMapper.GetInstance( input.CompilationModel.RoslynCompilation );

            // TODO: Merge observable and non-observable transformations so that the order is preserved.
            //       Maybe have all transformations already together in the input?
            var allTransformations =
                input.CompilationModel.GetAllObservableTransformations()
                    .SelectMany( x => x.Transformations )
                    .OfType<ISyntaxTreeTransformation>()
                    .Concat( input.NonObservableTransformations.OfType<ISyntaxTreeTransformation>() )
                    .ToList();

            // Visit all member introductions, respect aspect part ordering.
            foreach ( var memberIntroduction in allTransformations.OfType<IMemberIntroduction>() )
            {
                var introductionContext = new MemberIntroductionContext(
                    diagnostics,
                    nameProvider,
                    lexicalScopeHelper.GetLexicalScope( memberIntroduction ),
                    syntaxFactory,
                    this._serviceProvider );

                var introducedMembers = memberIntroduction.GetIntroducedMembers( introductionContext );

                introducedCollection.Add( memberIntroduction, introducedMembers );
            }

            // Visit all interface introductions.
            foreach ( var interfaceIntroduction in allTransformations.OfType<IInterfaceImplementationIntroduction>() )
            {
                var introducedInterfaces = interfaceIntroduction.GetIntroducedInterfaceImplementations();

                introducedCollection.Add( interfaceIntroduction, introducedInterfaces );
            }

            // Group diagnostic suppressions by target.
            var suppressionsByTarget = input.DiagnosticSuppressions.ToMultiValueDictionary(
                s => s.Declaration,
                input.CompilationModel.InvariantComparer );

            // Process syntax trees one by one.
            var intermediateCompilation = input.InitialCompilation;
            Rewriter addIntroducedElementsRewriter = new( introducedCollection, suppressionsByTarget, input.CompilationModel );

            foreach ( var initialSyntaxTree in input.InitialCompilation.SyntaxTrees )
            {
                var oldRoot = initialSyntaxTree.GetRoot();
                var newRoot = addIntroducedElementsRewriter.Visit( oldRoot );

                if ( oldRoot != newRoot )
                {
                    // TODO: Add an annotation to modified syntax roots so that they can be differentiated from unmodified ones and skipped by the next visitors.
                    var intermediateSyntaxTree = initialSyntaxTree.WithRootAndOptions( newRoot, initialSyntaxTree.Options );

                    syntaxTreeMapping.Add( initialSyntaxTree, intermediateSyntaxTree );
                }
            }

            intermediateCompilation = intermediateCompilation.UpdateSyntaxTrees(
                syntaxTreeMapping.Select( p => (p.Key, p.Value) ).ToList(),
                Array.Empty<SyntaxTree>() );

            var introductionRegistry = new LinkerIntroductionRegistry(
                input.CompilationModel,
                intermediateCompilation.Compilation,
                syntaxTreeMapping,
                introducedCollection.IntroducedMembers );

            return new LinkerIntroductionStepOutput( diagnostics.ToImmutable(), intermediateCompilation, introductionRegistry, input.OrderedAspectLayers );
        }
    }
}