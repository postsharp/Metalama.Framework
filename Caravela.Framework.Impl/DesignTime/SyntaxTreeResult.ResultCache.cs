// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.DesignTime
{
    internal class SyntaxTreeResult
    {
        public SyntaxTree SyntaxTree { get; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public ImmutableArray<ScopedSuppression> Suppressions { get; }

        public ImmutableArray<IntroducedSyntaxTree> Introductions { get; }
        
        /// <summary>
        /// Gets the list of syntax trees (identified by FilePath) on which the current
        /// syntax tree is dependent. 
        /// </summary>
        public ImmutableArray<string> Dependencies { get; }

        public SyntaxTreeResult(
            SyntaxTree syntaxTree,
            ImmutableArray<Diagnostic>? diagnostics,
            ImmutableArray<ScopedSuppression>? suppressions,
            ImmutableArray<IntroducedSyntaxTree>? introductions,
            ImmutableArray<string>? dependencies)
        {
            this.SyntaxTree = syntaxTree;
            this.Diagnostics = diagnostics ?? ImmutableArray<Diagnostic>.Empty;
            this.Suppressions = suppressions ?? ImmutableArray<ScopedSuppression>.Empty;
            this.Introductions = introductions ?? ImmutableArray<IntroducedSyntaxTree>.Empty;
            this.Dependencies = dependencies ?? ImmutableArray<string>.Empty;
        }
    }
}