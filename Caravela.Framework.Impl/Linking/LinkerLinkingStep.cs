// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Impl.AspectOrdering;
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
        //   * A, which contains only a call A1.
        //   * A1, which replaces the annotated call with a call to A2.
        //   * A2, which replaced the annotatated call with a call to Ao.
        //   * Ao, which contains the original method body.
        //
        // When inlining we need to know whether A1, A2 and Ao will be called from multiple places. This information is coming from analysis phase.
        //
        // Ideal inlining result is a single method A, which will contain logic from all aspects and the original method.

        private readonly DiagnosticList _diagnostics;
        private readonly IReadOnlyList<AspectLayer> _orderedAspectLayers;
        private readonly LinkerTransformationRegistry _transformationRegistry;
        private readonly CSharpCompilation _intermediateCompilation;
        private readonly LinkerAnalysisRegistry _referenceRegistry;

        public LinkerLinkingStep( IReadOnlyList<AspectLayer> orderedAspectLayers, LinkerTransformationRegistry transformationRegistry, CSharpCompilation intermediateCompilation, LinkerAnalysisRegistry referenceRegistry )
        {
            this._orderedAspectLayers = orderedAspectLayers;
            this._transformationRegistry = transformationRegistry;
            this._intermediateCompilation = intermediateCompilation;
            this._referenceRegistry = referenceRegistry;
            this._diagnostics = new DiagnosticList();
        }

        public static LinkerLinkingStep Create( IReadOnlyList<AspectLayer> orderedAspectLayers, LinkerTransformationRegistry transformationRegistry, CSharpCompilation intermediateCompilation, LinkerAnalysisRegistry referenceRegistry )
        {
            return new LinkerLinkingStep( orderedAspectLayers, transformationRegistry, intermediateCompilation, referenceRegistry );
        }

        public LinkerLinkingStepResult Execute()
        {
            var finalCompilation = this._intermediateCompilation;
            var rewriter = new LinkingRewriter( this._orderedAspectLayers, this._transformationRegistry, this._intermediateCompilation, this._referenceRegistry );

            foreach ( var syntaxTree in finalCompilation.SyntaxTrees )
            {
                // Run the linking rewriter.
                var newRoot = rewriter.Visit( syntaxTree.GetRoot() );

                var newSyntaxTree = syntaxTree.WithRootAndOptions( newRoot, syntaxTree.Options );

                finalCompilation = finalCompilation.ReplaceSyntaxTree( syntaxTree, newSyntaxTree );
            }

            return new LinkerLinkingStepResult( finalCompilation, this._diagnostics );
        }
    }
}
