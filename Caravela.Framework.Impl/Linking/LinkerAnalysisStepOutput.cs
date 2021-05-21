// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Output of the linker analysis.
    /// </summary>
    internal class LinkerAnalysisStepOutput
    {
        public LinkerAnalysisStepOutput(
            ImmutableUserDiagnosticList diagnostics,
            PartialCompilation intermediateCompilation,
            LinkerAnalysisRegistry analysisRegistry )
        {
            this.Diagnostics = diagnostics;
            this.IntermediateCompilation = intermediateCompilation;
            this.AnalysisRegistry = analysisRegistry;
        }

        /// <summary>
        /// Gets diagnostic sink.
        /// </summary>
        public ImmutableUserDiagnosticList Diagnostics { get; }

        /// <summary>
        /// Gets the intermediate compilation (produced in introduction step).
        /// </summary>
        public PartialCompilation IntermediateCompilation { get; }

        /// <summary>
        /// Gets the analysis registry.
        /// </summary>
        public LinkerAnalysisRegistry AnalysisRegistry { get; }
    }
}