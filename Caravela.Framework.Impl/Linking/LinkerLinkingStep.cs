using System.Collections.Generic;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class LinkerLinkingStep
    {
        // Linking step does two things:
        //   * Transforms introduced code so that aspects use correct declarations that are consistent with aspect layer order.
        //   * When possible inlines code that is present in separate method after introduction phase.
        //
        // For purposes of inlining, this step expects that code in all overrides have non-conflicting names. This should be managed in introduction phase.
        //
        // For example if we have method A with overrides A1 and A2, the intermediate compilation contains three entities:
        //   * A, which is the original method.
        //   * A1, which contains annotated call to A.
        //   * A2, which contains annotated call to A.
        //
        // Uninlined linked code (if no inlining is possible) contains:
        //   * A, which calls A1.
        //   * A1, which calls A2.
        //   * A2, which calls Ao.
        //   * Ao, which contains the original method body.
        //
        // When inlining we need to know whether A1, A2 and Ao will be called from multiple places. This information is coming from analysis phase.
        //
        // Ideal inlined call will be a single method A, which will contain logic from all aspects.

        private readonly DiagnosticList _diagnostics;
        private readonly IReadOnlyList<AspectPart> _orderedAspectParts;
        private readonly LinkerTransformationRegistry _transformationRegistry;
        private readonly CSharpCompilation _intermediateCompilation;
        private readonly LinkerReferenceRegistry _referenceRegistry;

        public LinkerLinkingStep( IReadOnlyList<AspectPart> orderedAspectParts, LinkerTransformationRegistry transformationRegistry, CSharpCompilation intermediateCompilation, LinkerReferenceRegistry referenceRegistry )
        {
            this._orderedAspectParts = orderedAspectParts;
            this._transformationRegistry = transformationRegistry;
            this._intermediateCompilation = intermediateCompilation;
            this._referenceRegistry = referenceRegistry;
        }

        public static LinkerLinkingStep Create( IReadOnlyList<AspectPart> orderedAspectParts, LinkerTransformationRegistry transformationRegistry, CSharpCompilation intermediateCompilation, LinkerReferenceRegistry referenceRegistry )
        {
            return new LinkerLinkingStep( orderedAspectParts, transformationRegistry, intermediateCompilation, referenceRegistry );
        }

        public LinkerLinkingStepResult Execute()
        {
            var finalCompilation = this._intermediateCompilation;
            var rewriter = new LinkingRewriter( this._orderedAspectParts, this._transformationRegistry, this._intermediateCompilation, this._referenceRegistry );

            foreach ( var syntaxTree in finalCompilation.SyntaxTrees )
            {
                var newRoot = rewriter.Visit( syntaxTree.GetRoot() );

                var newSyntaxTree = syntaxTree.WithRootAndOptions( newRoot, syntaxTree.Options );

                finalCompilation = finalCompilation.ReplaceSyntaxTree( syntaxTree, newSyntaxTree );
            }

            return new LinkerLinkingStepResult( finalCompilation, this._diagnostics );
        }
    }
}
