// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Output of the linker analysis.
    /// </summary>
    internal class LinkerAnalysisStepOutput
    {
        public LinkerAnalysisStepOutput(
            UserDiagnosticSink diagnosticSink,
            PartialCompilation intermediateCompilation,
            LinkerIntroductionRegistry introductionRegistry,
            LinkerAnalysisRegistry analysisRegistry,
            AspectReferenceResolver referenceResolver )
        {
            this.DiagnosticSink = diagnosticSink;
            this.IntermediateCompilation = intermediateCompilation;
            this.IntroductionRegistry = introductionRegistry;
            this.AnalysisRegistry = analysisRegistry;
            this.ReferenceResolver = referenceResolver;
        }

        /// <summary>
        /// Gets diagnostic sink.
        /// </summary>
        public UserDiagnosticSink DiagnosticSink { get; }

        /// <summary>
        /// Gets the intermediate compilation (produced in introduction step).
        /// </summary>
        public PartialCompilation IntermediateCompilation { get; }

        /// <summary>
        /// Gets the introduction registry.
        /// </summary>
        public LinkerIntroductionRegistry IntroductionRegistry { get; }

        /// <summary>
        /// Gets the analysis registry.
        /// </summary>
        public LinkerAnalysisRegistry AnalysisRegistry { get; }

        public AspectReferenceResolver ReferenceResolver { get; }
    }
}