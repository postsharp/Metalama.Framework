using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl
{
    internal sealed class PipelineStageResult
    {
        public CSharpCompilation Compilation { get; }

        public IReadOnlyList<Diagnostic> Diagnostics { get; }

        public IReadOnlyList<ResourceDescription> Resources { get; }

        public IReadOnlyList<IAspectSource> AspectSources { get; }

        /// <summary>
        /// Gets the list of syntax trees to be added to the compilation (typically in a source generation scenario). The key is the "hint name" in the
        /// source generator API.
        /// </summary>
        public IImmutableDictionary<string, SyntaxTree> AdditionalSyntaxTrees { get; }

        /// <summary>
        /// Gets the list of ordered aspect parts.
        /// </summary>
        public IReadOnlyList<AspectPart> AspectParts { get; }

        public PipelineStageResult(
            CSharpCompilation compilation,
            IReadOnlyList<AspectPart> aspectParts,
            IReadOnlyList<Diagnostic>? diagnostics = null,
            IReadOnlyList<ResourceDescription>? resources = null,
            IReadOnlyList<IAspectSource>? aspectSources = null,
            IImmutableDictionary<string, SyntaxTree>? additionalSyntaxTrees = null )
        {
            this.Compilation = compilation;
            this.Diagnostics = diagnostics ?? Array.Empty<Diagnostic>();
            this.Resources = resources ?? Array.Empty<ResourceDescription>();
            this.AspectSources = aspectSources ?? Array.Empty<IAspectSource>();
            this.AspectParts = aspectParts;
            this.AdditionalSyntaxTrees = additionalSyntaxTrees ?? ImmutableDictionary<string, SyntaxTree>.Empty;
        }
    }
}
