﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// Represents the content of <see cref="DesignTimeAspectPipelineResult"/>, but only the items that relate to a single <see cref="Microsoft.CodeAnalysis.SyntaxTree"/>.
    /// This class is compilation-independent and cacheable.
    /// </summary>
    internal class DesignTimeSyntaxTreeResult
    {
        /// <summary>
        /// Gets the <see cref="Microsoft.CodeAnalysis.SyntaxTree"/> for which the results was prepared.
        /// </summary>
        public SyntaxTree SyntaxTree { get; }

        /// <summary>
        /// Gets or sets the <see cref="Microsoft.CodeAnalysis.SyntaxTree"/> that was last used to diff the original <see cref="SyntaxTree"/> with
        /// the current one. 
        /// </summary>
        public SyntaxTree LastComparedSyntaxTree { get; set; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public ImmutableArray<CacheableScopedSuppression> Suppressions { get; }

        public ImmutableArray<IntroducedSyntaxTree> Introductions { get; }

        /// <summary>
        /// Gets the list of syntax trees (identified by FilePath) on which the current
        /// syntax tree is dependent. 
        /// </summary>
        public ImmutableArray<string> Dependencies { get; }

        public DesignTimeSyntaxTreeResult(
            SyntaxTree syntaxTree,
            ImmutableArray<Diagnostic>? diagnostics,
            ImmutableArray<CacheableScopedSuppression>? suppressions,
            ImmutableArray<IntroducedSyntaxTree>? introductions,
            ImmutableArray<string>? dependencies )
        {
            this.SyntaxTree = this.LastComparedSyntaxTree = syntaxTree;
            this.Diagnostics = diagnostics ?? ImmutableArray<Diagnostic>.Empty;
            this.Suppressions = suppressions ?? ImmutableArray<CacheableScopedSuppression>.Empty;
            this.Introductions = introductions ?? ImmutableArray<IntroducedSyntaxTree>.Empty;
            this.Dependencies = dependencies ?? ImmutableArray<string>.Empty;
        }
    }
}