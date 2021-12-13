﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Validation;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// Contains the inputs and the outputs of a <see cref="PipelineStage"/>.
    /// </summary>
    internal sealed class PipelineStageResult
    {
        public ProjectModel Project { get; }

        /// <summary>
        /// Gets the resulting Roslyn compilation.
        /// </summary>
        public PartialCompilation Compilation { get; }

        /// <summary>
        /// Gets the set of diagnostics.
        /// </summary>
        public ImmutableUserDiagnosticList Diagnostics { get; }

        /// <summary>
        /// Gets the list of aspect sources.
        /// </summary>
        public ImmutableArray<IAspectSource> AspectSources { get; }

        public ImmutableArray<ValidatorSource> ValidatorSources { get; }

        /// <summary>
        /// Gets the list of syntax trees to be added to the compilation (typically in a source generation scenario). The key is the "hint name" in the
        /// source generator API.
        /// </summary>
        public ImmutableArray<IntroducedSyntaxTree> AdditionalSyntaxTrees { get; }

        /// <summary>
        /// Gets the list of ordered aspect parts.
        /// </summary>
        public ImmutableArray<OrderedAspectLayer> AspectLayers { get; }

        public ImmutableArray<IAspectInstance> ExternallyInheritableAspects { get; }

        /// <summary>
        /// Gets the compilation model corresponding to <see cref="Compilation"/>, if it has been created.
        /// </summary>
        public ImmutableArray<CompilationModel> CompilationModels { get; }

        public CompilationModel? FirstCompilationModel { get; }

        public ImmutableArray<AdditionalCompilationOutputFile> AdditionalCompilationOutputFiles { get; }

        public PipelineStageResult(
            PartialCompilation compilation,
            ProjectModel project,
            ImmutableArray<OrderedAspectLayer> aspectLayers,
            ImmutableArray<CompilationModel> compilationModels,
            ImmutableUserDiagnosticList? diagnostics = null,
            ImmutableArray<IAspectSource> aspectSources = default,
            ImmutableArray<ValidatorSource> validatorSources = default,
            ImmutableArray<IAspectInstance> inheritableAspectInstances = default,
            ImmutableArray<IntroducedSyntaxTree> additionalSyntaxTrees = default,
            ImmutableArray<AdditionalCompilationOutputFile> additionalCompilationOutputFiles = default )
        {
            this.Compilation = compilation;
            this.Diagnostics = diagnostics ?? ImmutableUserDiagnosticList.Empty;
            this.AspectSources = aspectSources.IsDefault ? ImmutableArray<IAspectSource>.Empty : aspectSources;
            this.ValidatorSources = validatorSources.IsDefault ? ImmutableArray<ValidatorSource>.Empty : validatorSources;
            this.AspectLayers = aspectLayers;
            this.CompilationModels = compilationModels;
            this.ExternallyInheritableAspects = inheritableAspectInstances.IsDefault ? ImmutableArray<IAspectInstance>.Empty : inheritableAspectInstances;
            this.Project = project;
            this.AdditionalSyntaxTrees = additionalSyntaxTrees.IsDefault ? ImmutableArray<IntroducedSyntaxTree>.Empty : additionalSyntaxTrees;

            this.AdditionalCompilationOutputFiles = additionalCompilationOutputFiles.IsDefault
                ? ImmutableArray<AdditionalCompilationOutputFile>.Empty
                : additionalCompilationOutputFiles;
        }
    }
}