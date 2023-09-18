// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline
{
    /// <summary>
    /// Represents the content of <see cref="DesignTimePipelineExecutionResult"/>, but only the items that relate to a single <see cref="Microsoft.CodeAnalysis.SyntaxTree"/>.
    /// This class is compilation-independent and cacheable.
    /// </summary>
    internal sealed partial class SyntaxTreePipelineResult
    {
        /// <summary>
        /// Gets the <see cref="Microsoft.CodeAnalysis.SyntaxTree"/> for which the results was prepared.
        /// </summary>
        public SyntaxTree SyntaxTree { get; }

        public ImmutableArray<Diagnostic> Diagnostics { get; }

        public ImmutableArray<CacheableScopedSuppression> Suppressions { get; }

        public ImmutableArray<IntroducedSyntaxTree> Introductions { get; }

        // TODO: It seems the Dependencies property should be used when the syntax tree is updated.
        // ReSharper disable once UnusedAutoPropertyAccessor.Global,  MemberCanBePrivate.Global

        /// <summary>
        /// Gets the list of syntax trees (identified by FilePath) on which the current
        /// syntax tree is dependent. 
        /// </summary>
        public ImmutableArray<string> Dependencies { get; }

        public ImmutableArray<InheritableAspectInstance> InheritableAspects { get; }

        public ImmutableArray<InheritableOptionsInstance> InheritableOptions { get; }

        public ImmutableArray<DesignTimeReferenceValidatorInstance> ReferenceValidators { get; }

        public ImmutableArray<DesignTimeAspectInstance> AspectInstances { get; }

        public ImmutableArray<DesignTimeTransformation> Transformations { get; }

        private SyntaxTreePipelineResult(
            SyntaxTree syntaxTree,
            ImmutableArray<Diagnostic>? diagnostics,
            ImmutableArray<CacheableScopedSuppression>? suppressions,
            ImmutableArray<IntroducedSyntaxTree>? introductions,
            ImmutableArray<string>? dependencies,
            ImmutableArray<InheritableAspectInstance>? inheritableAspects,
            ImmutableArray<DesignTimeReferenceValidatorInstance>? validators,
            ImmutableArray<DesignTimeAspectInstance>? aspectInstances,
            ImmutableArray<DesignTimeTransformation>? transformations,
            ImmutableArray<InheritableOptionsInstance>? inheritableOptions )
        {
            this.SyntaxTree = syntaxTree;
            this.InheritableOptions = inheritableOptions ?? ImmutableArray<InheritableOptionsInstance>.Empty;
            this.ReferenceValidators = validators ?? ImmutableArray<DesignTimeReferenceValidatorInstance>.Empty;
            this.InheritableAspects = inheritableAspects ?? ImmutableArray<InheritableAspectInstance>.Empty;
            this.Diagnostics = diagnostics ?? ImmutableArray<Diagnostic>.Empty;
            this.Suppressions = suppressions ?? ImmutableArray<CacheableScopedSuppression>.Empty;
            this.Introductions = introductions ?? ImmutableArray<IntroducedSyntaxTree>.Empty;
            this.Dependencies = dependencies ?? ImmutableArray<string>.Empty;
            this.AspectInstances = aspectInstances ?? ImmutableArray<DesignTimeAspectInstance>.Empty;
            this.Transformations = transformations ?? ImmutableArray<DesignTimeTransformation>.Empty;
        }

        public override string ToString()
            => $"FilePath='{this.SyntaxTree.FilePath}, Diagnostics={this.Diagnostics.Length}, Suppressions={this.Suppressions.Length}, Introductions={this.Introductions.Length}, InheritableAspects={this.InheritableAspects.Length}";
    }
}