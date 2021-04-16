// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Impl.Transformations;
using Microsoft.CodeAnalysis;

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
    /// Aspect linker's introduction steps. Adds introduced members from all transformation to the Roslyn compilation. This involves calling template expansion.
    /// This results in the transformation registry and intermediate compilation, and also produces diagnostics.
    /// </summary>
    internal partial class LinkerIntroductionStep : AspectLinkerPipelineStep<AspectLinkerInput, LinkerIntroductionStepOutput>
    {
        public static LinkerIntroductionStep Instance { get; } = new LinkerIntroductionStep();

        private LinkerIntroductionStep()
        {
        }

        public override LinkerIntroductionStepOutput Execute( AspectLinkerInput input )
        {
            var diagnostics = new DiagnosticList( null );
            var nameProvider = new LinkerIntroductionNameProvider();
            var proceedImplFactory = new LinkerProceedImplementationFactory();
            var lexicalScopeHelper = new LexicalScopeHelper();
            var introducedMemberCollection = new IntroducedMemberCollection();
            var syntaxTreeMapping = new Dictionary<SyntaxTree, SyntaxTree>();

            // TODO: Merge observable and non-observable transformations so that the order is preserved.
            //       Maybe have all transformations already together in the input?
            var allTransformations =
                input.FinalCompilationModel.GetAllObservableTransformations()
                .SelectMany( x => x.Transformations )
                .OfType<ISyntaxTreeTransformation>()
                .Concat( input.NonObservableTransformations.OfType<ISyntaxTreeTransformation>() )
                .ToList();

            // Visit all introductions, respect aspect part ordering.
            foreach ( var memberIntroduction in allTransformations.OfType<IMemberIntroduction>() )
            {
                var introductionContext = new MemberIntroductionContext( diagnostics, nameProvider, lexicalScopeHelper.GetLexicalScope( memberIntroduction ), proceedImplFactory );
                var introducedMembers = memberIntroduction.GetIntroducedMembers( introductionContext );

                introducedMemberCollection.Add( memberIntroduction, introducedMembers );
            }

            var intermediateCompilation = input.InitialCompilation;

            // Process syntax trees one by one.
            Rewriter addIntroducedElementsRewriter = new( introducedMemberCollection, diagnostics );

            foreach ( var initialSyntaxTree in input.InitialCompilation.SyntaxTrees )
            {
                var newRoot = addIntroducedElementsRewriter.Visit( initialSyntaxTree.GetRoot() );

#if DEBUG
                // Improve readibility of intermediate compilation in debug builds.
                newRoot = SyntaxNodeExtensions.NormalizeWhitespace( newRoot );
#endif

                var intermediateSyntaxTree = initialSyntaxTree.WithRootAndOptions( newRoot, initialSyntaxTree.Options );

                syntaxTreeMapping.Add( initialSyntaxTree, intermediateSyntaxTree );
                intermediateCompilation = intermediateCompilation.ReplaceSyntaxTree( initialSyntaxTree, intermediateSyntaxTree );
            }

            var introductionRegistry = new LinkerIntroductionRegistry( input.FinalCompilationModel, intermediateCompilation, syntaxTreeMapping, introducedMemberCollection.IntroducedMembers );

            return new LinkerIntroductionStepOutput( diagnostics, intermediateCompilation, introductionRegistry, input.OrderedAspectLayers );
        }

        private class LexicalScopeHelper
        {
            private readonly Dictionary<ICodeElement, LinkerLexicalScope> _scopes = new Dictionary<ICodeElement, LinkerLexicalScope>();

            public ITemplateExpansionLexicalScope GetLexicalScope( IMemberIntroduction introduction )
            {
                // TODO: This will need to be changed for other transformations than methods.

                if ( introduction is IOverriddenElement overriddenElement )
                {
                    if ( !this._scopes.TryGetValue( overriddenElement.OverriddenElement, out var lexicalScope ) )
                    {
                        this._scopes[overriddenElement.OverriddenElement] = lexicalScope =
                            LinkerLexicalScope.CreateEmpty( LinkerLexicalScope.CreateFromElement( overriddenElement.OverriddenElement ) );

                        return lexicalScope;
                    }

                    this._scopes[overriddenElement.OverriddenElement] = lexicalScope = LinkerLexicalScope.CreateEmpty( lexicalScope.GetTransitiveClosure() );
                    return lexicalScope;
                }
                else
                {
                    // For other member types we need to create empty lexical scope.
                    return LinkerLexicalScope.CreateEmpty();
                }
            }
        }
    }
}
