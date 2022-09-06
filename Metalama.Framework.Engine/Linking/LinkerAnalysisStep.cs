// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Linking.Inlining;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Analysis step of the linker, main goal of which is to produce LinkerAnalysisRegistry.
    /// </summary>
    internal partial class LinkerAnalysisStep : AspectLinkerPipelineStep<LinkerIntroductionStepOutput, LinkerAnalysisStepOutput>
    {
        public static LinkerAnalysisStep Instance { get; } = new();

        private LinkerAnalysisStep() { }

        public override LinkerAnalysisStepOutput Execute( LinkerIntroductionStepOutput input )
        {
            var inlinerProvider = new InlinerProvider();

            var referenceResolver = new AspectReferenceResolver(
                input.IntroductionRegistry,
                input.OrderedAspectLayers,
                input.FinalCompilationModel,
                input.IntermediateCompilation.Compilation );

            // Analyze method bodies (aspect references & flow).
            var methodBodyAnalyzer = new MethodBodyAnalyzer(
                input.IntermediateCompilation,
                input.IntroductionRegistry,
                referenceResolver );

            var methodBodyAnalysisResults = methodBodyAnalyzer.GetMethodBodyAnalysisResults();

            Dictionary<AspectReferenceTarget, List<ResolvedAspectReference>> aspectReferenceIndexBuilder = new();

            foreach ( var methodBodyAnalysisResult in methodBodyAnalysisResults )
            {
                foreach ( var aspectReference in methodBodyAnalysisResult.Value.AspectReferences )
                {
                    var key = new AspectReferenceTarget(
                        aspectReference.ResolvedSemantic.Symbol,
                        aspectReference.ResolvedSemantic.Kind,
                        aspectReference.Specification.TargetKind );

                    if ( !aspectReferenceIndexBuilder.TryGetValue( key, out var list ) )
                    {
                        aspectReferenceIndexBuilder[key] = list = new List<ResolvedAspectReference>();
                    }

                    list.Add( aspectReference );
                }
            }

            var aspectReferenceIndex = aspectReferenceIndexBuilder.ToDictionary( x => x.Key, x => (IReadOnlyList<ResolvedAspectReference>) x.Value );

            var reachabilityAnalyzer = new ReachabilityAnalyzer(
                input.IntroductionRegistry,
                methodBodyAnalysisResults );

            var reachableSemantics = reachabilityAnalyzer.AnalyzeReachability();

            var inlineabilityAnalyzer = new InlineabilityAnalyzer(
                input.IntermediateCompilation,
                input.IntroductionRegistry,
                reachableSemantics,
                inlinerProvider,
                aspectReferenceIndex );

            var inliningSpecifications = inlineabilityAnalyzer.GetInlineableSymbols().ToList();

            var analysisRegistry = new LinkerAnalysisRegistry(
                methodBodyAnalysisResults,
                reachableSemantics,
                inliningSpecifications );

            return new LinkerAnalysisStepOutput(
                input.DiagnosticSink,
                input.IntermediateCompilation,
                input.IntroductionRegistry,
                analysisRegistry,
                referenceResolver,
                input.ProjectOptions );
        }
    }
}