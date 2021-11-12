﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Contains the inputs and the outputs of a <see cref="PipelineStage"/>.
    /// </summary>
    internal sealed class PipelineStageResult
    {
        public ProjectModel Project { get; }

        /// <summary>
        /// Gets the Roslyn compilation.
        /// </summary>
        public PartialCompilation PartialCompilation { get; }

        /// <summary>
        /// Gets the set of diagnostics.
        /// </summary>
        public ImmutableUserDiagnosticList Diagnostics { get; }

        /// <summary>
        /// Gets the list of aspect sources.
        /// </summary>
        public IReadOnlyList<IAspectSource> AspectSources { get; }

        /// <summary>
        /// Gets the list of syntax trees to be added to the compilation (typically in a source generation scenario). The key is the "hint name" in the
        /// source generator API.
        /// </summary>
        public IReadOnlyList<IntroducedSyntaxTree> AdditionalSyntaxTrees { get; }

        /// <summary>
        /// Gets the list of ordered aspect parts.
        /// </summary>
        public ImmutableArray<OrderedAspectLayer> AspectLayers { get; }

        public ImmutableArray<AttributeAspectInstance> ExternallyInheritableAspects { get; }

        public ImmutableArray<AuxiliaryFile> AuxiliaryFiles { get; }

        public PipelineStageResult(
            PartialCompilation compilation,
            ProjectModel project,
            ImmutableArray<OrderedAspectLayer> aspectLayers,
            ImmutableUserDiagnosticList? diagnostics = null,
            IReadOnlyList<IAspectSource>? aspectSources = null,
            ImmutableArray<AttributeAspectInstance>? inheritableAspectInstances = null,
            IReadOnlyList<IntroducedSyntaxTree>? additionalSyntaxTrees = null,
            ImmutableArray<AuxiliaryFile>? auxiliaryFiles = null )
        {
            this.PartialCompilation = compilation;
            this.Diagnostics = diagnostics ?? ImmutableUserDiagnosticList.Empty;
            this.AspectSources = aspectSources ?? Array.Empty<IAspectSource>();
            this.AspectLayers = aspectLayers;
            this.ExternallyInheritableAspects = inheritableAspectInstances ?? ImmutableArray<AttributeAspectInstance>.Empty;
            this.Project = project;
            this.AdditionalSyntaxTrees = additionalSyntaxTrees ?? ImmutableArray<IntroducedSyntaxTree>.Empty;
            this.AuxiliaryFiles = auxiliaryFiles ?? ImmutableArray<AuxiliaryFile>.Empty;
        }
    }
}