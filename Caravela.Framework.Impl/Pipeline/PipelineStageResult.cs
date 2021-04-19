// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        /// <summary>
        /// Gets the Roslyn compilation.
        /// </summary>
        public CSharpCompilation Compilation { get; }

        /// <summary>
        /// Gets the set of diagnostics.
        /// </summary>
        public ImmutableDiagnosticList Diagnostics { get; }

        /// <summary>
        /// Gets the list of managed resources.
        /// </summary>
        public IReadOnlyList<ResourceDescription> Resources { get; }

        /// <summary>
        /// Gets the list of aspect sources.
        /// </summary>
        public IReadOnlyList<IAspectSource> AspectSources { get; }

        /// <summary>
        /// Gets the list of syntax trees to be added to the compilation (typically in a source generation scenario). The key is the "hint name" in the
        /// source generator API.
        /// </summary>
        public IImmutableDictionary<string, SyntaxTree> AdditionalSyntaxTrees { get; }

        /// <summary>
        /// Gets the list of ordered aspect parts.
        /// </summary>
        public IReadOnlyList<OrderedAspectLayer> AspectLayers { get; }

        public PipelineStageResult(
            CSharpCompilation compilation,
            IReadOnlyList<OrderedAspectLayer> aspectLayers,
            ImmutableDiagnosticList? diagnostics = null,
            IReadOnlyList<ResourceDescription>? resources = null,
            IReadOnlyList<IAspectSource>? aspectSources = null,
            IImmutableDictionary<string, SyntaxTree>? additionalSyntaxTrees = null )
        {
            this.Compilation = compilation;
            this.Diagnostics = diagnostics ?? ImmutableDiagnosticList.Empty;
            this.Resources = resources ?? Array.Empty<ResourceDescription>();
            this.AspectSources = aspectSources ?? Array.Empty<IAspectSource>();
            this.AspectLayers = aspectLayers;
            this.AdditionalSyntaxTrees = additionalSyntaxTrees ?? ImmutableDictionary<string, SyntaxTree>.Empty;
        }
    }
}