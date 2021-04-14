// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Output of the linker's analysis.
    /// </summary>
    internal class LinkerAnalysisStepOutput
    {
        public LinkerAnalysisStepOutput( ImmutableDiagnosticList diagnostics, CSharpCompilation intermediateCompilation, LinkerAnalysisRegistry analysisRegistry )
        {
            this.Diagnostics = diagnostics;
            this.IntermediateCompilation = intermediateCompilation;
            this.AnalysisRegistry = analysisRegistry;
        }

        /// <summary>
        /// Gets diagnostic sink.
        /// </summary>
        public ImmutableDiagnosticList Diagnostics { get; }

        /// <summary>
        /// Gets the intermediate compilation (produced in introduction step).
        /// </summary>
        public CSharpCompilation IntermediateCompilation { get; }

        /// <summary>
        /// Gets the analysis registry.
        /// </summary>
        public LinkerAnalysisRegistry AnalysisRegistry { get; }
    }
}