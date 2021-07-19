﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking.Inlining;
using Caravela.Framework.Sdk;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
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
    // Non-inlined linked code (if no inlining is possible) contains:
    //   * A, which contains only a call A1.
    //   * A1, which replaces the annotated call with a call to A2.
    //   * A2, which replaced the annotated call with a call to Ao.
    //   * Ao, which contains the original method body.
    //
    // When inlining we need to know whether A1, A2 and Ao will be called from multiple places. This information is coming from analysis phase.
    //
    // Ideal inlining result is a single method A, which will contain logic from all aspects and the original method.

    /// <summary>
    /// Linker linking step, which rewrites the intermediate compilation and produces the final compilation. 
    /// </summary>
    internal partial class LinkerLinkingStep : AspectLinkerPipelineStep<LinkerAnalysisStepOutput, AspectLinkerResult>
    {
        public static LinkerLinkingStep Instance { get; } = new();

        private LinkerLinkingStep() { }

        public override AspectLinkerResult Execute( LinkerAnalysisStepOutput input )
        {
            var inliners = new Inliner[]
            {
                new MethodAssignmentInliner(),
                new MethodLocalDeclarationInliner(),
                new MethodReturnStatementInliner(),
                new MethodCastReturnStatementInliner(),
                new MethodInvocationInliner(),
                new MethodDiscardInliner(),
                new PropertyGetAssignmentInliner(),
                new PropertyGetReturnInliner(),
                new PropertyGetCastReturnInliner(),
                new PropertyGetLocalDeclarationInliner(),
                new PropertySetValueAssignmentInliner(),
                new EventAddAssignmentInliner(),
                new EventRemoveAssignmentInliner()
            };

            var rewritingDriver = new LinkerRewritingDriver(
                input.IntermediateCompilation.Compilation,
                input.IntroductionRegistry,
                input.AnalysisRegistry,
                input.ReferenceResolver,
                input.DiagnosticSink,
                inliners );

            var linkingRewriter = new LinkingRewriter( input.IntermediateCompilation.Compilation, rewritingDriver );
            var cleanupRewriter = new CleanupRewriter();

            List<ModifiedSyntaxTree> replacedTrees = new();

            foreach ( var modifiedSyntaxTree in input.IntermediateCompilation.ModifiedSyntaxTrees )
            {
                var syntaxTree = modifiedSyntaxTree.Value.NewTree;

                // Run the linking rewriter for this tree.
                var linkedRoot = linkingRewriter.Visit( syntaxTree.GetRoot() );
                var cleanRoot = cleanupRewriter.Visit( linkedRoot );

                var newSyntaxTree = syntaxTree.WithRootAndOptions( cleanRoot, syntaxTree.Options );

                replacedTrees.Add( new ModifiedSyntaxTree( newSyntaxTree, syntaxTree ) );
            }

            return new AspectLinkerResult( input.IntermediateCompilation.UpdateSyntaxTrees( replacedTrees ), input.DiagnosticSink.ToImmutable() );
        }
    }
}