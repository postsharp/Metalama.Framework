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
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
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
    //   * Names(Subtree(R1)), Names(Subtree(R2)), Names(Subtree(R3)) are disjoint

    internal partial class LinkerIntroductionStep
    {
        // Transformations grouped by target syntax trees, preserved order.
        private readonly CSharpCompilation _initialCompilation;
        private readonly CompilationModel _finalCompilationModel;
        private readonly IReadOnlyList<ISyntaxTreeTransformation> _transformations;

        private LinkerIntroductionStep( CSharpCompilation initialCompilation, CompilationModel finalCompilationModel, IReadOnlyList<ISyntaxTreeTransformation> transformations )
        {
            this._initialCompilation = initialCompilation;
            this._transformations = transformations;
            this._finalCompilationModel = finalCompilationModel;
        }

        public static LinkerIntroductionStep Create( AspectLinkerInput input )
        {
            // TODO: Merge observable and non-observable transformations so that the order is preserved.
            //       Maybe have all transformations already together in the input?
            var allTransformations =
                input.CompilationModel.GetAllObservableTransformations()
                .SelectMany(x => x.Transformations)
                .OfType<ISyntaxTreeTransformation>()
                .Concat( input.NonObservableTransformations.OfType<ISyntaxTreeTransformation>() )
                .ToList();

            return new LinkerIntroductionStep( input.Compilation, input.CompilationModel, allTransformations );
        }

        public LinkerIntroductionStepOutput Execute()
        {
            var context = new Context( this._initialCompilation, this._finalCompilationModel );
            var diagnostics = new DiagnosticList( null );
            var nameProvider = new LinkerIntroductionNameProvider();
            var proceedImplFactory = new LinkerProceedImplementationFactory();

            // Visit all introductions, respect aspect part ordering.
            foreach ( var memberIntroduction in this._transformations.OfType<IMemberIntroduction>() )
            {
                var introductionContext = new MemberIntroductionContext( diagnostics, nameProvider, context.GetLexicalScope( memberIntroduction ), proceedImplFactory );
                var introducedMembers = memberIntroduction.GetIntroducedMembers( introductionContext );

                context.TransformationRegistry.SetIntroducedMembers( memberIntroduction, introducedMembers );
            }

            var intermediateCompilation = this._initialCompilation;

            // Process syntax trees one by one.
            foreach ( var initialSyntaxTree in this._initialCompilation.SyntaxTrees )
            {
                Rewriter addIntroducedElementsRewriter = new( context.TransformationRegistry, diagnostics );

                var newRoot = addIntroducedElementsRewriter.Visit( initialSyntaxTree.GetRoot() );
                var intermediateSyntaxTree = initialSyntaxTree.WithRootAndOptions( newRoot, initialSyntaxTree.Options );

                context.TransformationRegistry.SetIntermediateSyntaxTreeMapping( initialSyntaxTree, intermediateSyntaxTree );
                intermediateCompilation.ReplaceSyntaxTree( initialSyntaxTree, intermediateSyntaxTree );
            }

            // Push the intermediate compilation.
            context.TransformationRegistry.SetIntermediateCompilation( intermediateCompilation );

            // Freeze the introduction registry, it should not be changed after this point.
            context.TransformationRegistry.Freeze();

            return new LinkerIntroductionStepOutput( intermediateCompilation, context.TransformationRegistry );
        }

        private class Context
        {
            private readonly Dictionary<ICodeElement, LinkerLexicalScope> _lexicalScopeRegistry = new Dictionary<ICodeElement, LinkerLexicalScope>();

            public Dictionary<ICodeElement, LinkerLexicalScope> LexicalScopesByOverriddenElement { get; } = new Dictionary<ICodeElement, LinkerLexicalScope>();

            public LinkerTransformationRegistry TransformationRegistry { get; }

            public Compilation IntermediateCompilation { get; set; }

            public Context( Compilation initialCompilation, CompilationModel finalCompilationModel )
            {
                this.IntermediateCompilation = initialCompilation;
                this.TransformationRegistry = new LinkerTransformationRegistry( finalCompilationModel );
            }

            public void ReplaceSyntaxTree( SyntaxTree initialSyntaxTree, SyntaxTree intermediateSyntaxTree )
            {
                this.TransformationRegistry.SetIntermediateSyntaxTreeMapping( initialSyntaxTree, intermediateSyntaxTree );
                this.IntermediateCompilation = this.IntermediateCompilation.ReplaceSyntaxTree( initialSyntaxTree, intermediateSyntaxTree );
            }

            public ITemplateExpansionLexicalScope GetLexicalScope( IMemberIntroduction introduction )
            {
                // TODO: This will need to be changed for other transformations than methods.

                if ( introduction is IOverriddenElement overriddenElement )
                {
                    if ( !this._lexicalScopeRegistry.TryGetValue( overriddenElement.OverriddenElement, out var lexicalScope ) )
                    {
                        this._lexicalScopeRegistry[overriddenElement.OverriddenElement] = lexicalScope =
                            LinkerLexicalScope.CreateEmpty( LinkerLexicalScope.CreateFromMethod( (IMethodInternal) overriddenElement.OverriddenElement ) );

                        return lexicalScope;
                    }

                    this._lexicalScopeRegistry[overriddenElement.OverriddenElement] = lexicalScope = LinkerLexicalScope.CreateEmpty( lexicalScope.GetTransitiveClosure() );
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
