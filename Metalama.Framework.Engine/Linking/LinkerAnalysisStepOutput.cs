// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;

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
            LinkerCodeTransformationRegistry codeTransformationRegistry,
            AspectReferenceResolver referenceResolver,
            IProjectOptions? projectOptions )
        {
            this.DiagnosticSink = diagnosticSink;
            this.IntermediateCompilation = intermediateCompilation;
            this.IntroductionRegistry = introductionRegistry;
            this.AnalysisRegistry = analysisRegistry;
            this.CodeTransformationRegistry = codeTransformationRegistry;
            this.ReferenceResolver = referenceResolver;
            this.ProjectOptions = projectOptions;
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

        /// <summary>
        /// Gets the analysis registry.
        /// </summary>
        public LinkerCodeTransformationRegistry CodeTransformationRegistry { get; }

        /// <summary>
        /// Gets reference resolver.
        /// </summary>
        public AspectReferenceResolver ReferenceResolver { get; }

        /// <summary>
        /// Gets project options.
        /// </summary>
        public IProjectOptions? ProjectOptions { get; }
    }
}