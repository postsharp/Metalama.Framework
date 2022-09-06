﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking
{
    internal class LinkerIntroductionStepOutput
    {
        public LinkerIntroductionStepOutput(
            UserDiagnosticSink diagnosticSink,
            CompilationModel finalCompilationModel,
            PartialCompilation intermediateCompilation,
            LinkerIntroductionRegistry introductionRegistry,
            IReadOnlyList<OrderedAspectLayer> orderedAspectLayers,
            IProjectOptions? projectOptions )
        {
            this.DiagnosticSink = diagnosticSink;
            this.FinalCompilationModel = finalCompilationModel;
            this.IntermediateCompilation = intermediateCompilation;
            this.IntroductionRegistry = introductionRegistry;
            this.OrderedAspectLayers = orderedAspectLayers;
            this.ProjectOptions = projectOptions;
        }

        /// <summary>
        /// Gets the diagnostic sink.
        /// </summary>
        public UserDiagnosticSink DiagnosticSink { get; }

        /// <summary>
        /// Gets the final compilation model.
        /// </summary>
        public CompilationModel FinalCompilationModel { get; }

        /// <summary>
        /// Gets the intermediate compilation.
        /// </summary>
        public PartialCompilation IntermediateCompilation { get; }

        /// <summary>
        /// Gets the introduction registry.
        /// </summary>
        public LinkerIntroductionRegistry IntroductionRegistry { get; }

        /// <summary>
        /// Gets a list of ordered aspect layers.
        /// </summary>
        public IReadOnlyList<OrderedAspectLayer> OrderedAspectLayers { get; }

        /// <summary>
        /// Gets project options.
        /// </summary>
        public IProjectOptions? ProjectOptions { get; }
    }
}