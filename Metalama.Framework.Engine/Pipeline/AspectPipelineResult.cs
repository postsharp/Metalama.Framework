// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.AdditionalOutputs;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Validation;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// Contains the inputs and the outputs of a <see cref="PipelineStage"/>.
    /// </summary>
    [PublicAPI]
    public sealed class AspectPipelineResult
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
        internal ImmutableArray<IAspectSource> AspectSources { get; }

        internal ImmutableArray<IValidatorSource> ValidatorSources { get; }

        /// <summary>
        /// Gets the list of syntax trees to be added to the compilation (typically in a source generation scenario). The key is the "hint name" in the
        /// source generator API.
        /// </summary>
        public ImmutableArray<IntroducedSyntaxTree> AdditionalSyntaxTrees { get; }

        /// <summary>
        /// Gets the list of ordered aspect parts.
        /// </summary>
        internal ImmutableArray<OrderedAspectLayer> AspectLayers { get; }

        public ImmutableArray<IAspectInstance> ExternallyInheritableAspects { get; }

        public ImmutableArray<ReferenceValidatorInstance> ReferenceValidators { get; }

        public CompilationModel? FirstCompilationModel { get; }

        public CompilationModel? LastCompilationModel { get; }

        internal ImmutableArray<AspectInstanceResult> AspectInstanceResults { get; }

        public IReadOnlyList<IAspectInstance> AspectInstances => this.AspectInstanceResults.SelectAsImmutableArray( x => x.AspectInstance );

        public ImmutableArray<AdditionalCompilationOutputFile> AdditionalCompilationOutputFiles { get; }

        public ImmutableArray<ITransformationBase> Transformations { get; }

        internal AspectPipelineResult(
            PartialCompilation compilation,
            ProjectModel project,
            ImmutableArray<OrderedAspectLayer> aspectLayers,
            CompilationModel? firstCompilationModel,
            CompilationModel? lastCompilationModel,
            ImmutableUserDiagnosticList? diagnostics = null,
            ImmutableArray<IAspectSource> aspectSources = default,
            ImmutableArray<IValidatorSource> validatorSources = default,
            ImmutableArray<IAspectInstance> inheritableAspectInstances = default,
            ImmutableArray<ReferenceValidatorInstance> referenceValidators = default,
            ImmutableArray<IntroducedSyntaxTree> additionalSyntaxTrees = default,
            ImmutableArray<AspectInstanceResult> aspectInstanceResults = default,
            ImmutableArray<AdditionalCompilationOutputFile> additionalCompilationOutputFiles = default,
            ImmutableArray<ITransformationBase> transformations = default )
        {
            this.Compilation = compilation;
            this.Diagnostics = diagnostics ?? ImmutableUserDiagnosticList.Empty;
            this.AspectSources = aspectSources.IsDefault ? ImmutableArray<IAspectSource>.Empty : aspectSources;
            this.ValidatorSources = validatorSources.IsDefault ? ImmutableArray<IValidatorSource>.Empty : validatorSources;
            this.AspectLayers = aspectLayers;
            this.FirstCompilationModel = firstCompilationModel ?? lastCompilationModel;
            this.LastCompilationModel = lastCompilationModel;
            this.AspectInstanceResults = aspectInstanceResults.IsDefault ? ImmutableArray<AspectInstanceResult>.Empty : aspectInstanceResults;
            this.ExternallyInheritableAspects = inheritableAspectInstances.IsDefault ? ImmutableArray<IAspectInstance>.Empty : inheritableAspectInstances;

            this.ReferenceValidators =
                referenceValidators.IsDefault ? ImmutableArray<ReferenceValidatorInstance>.Empty : referenceValidators;

            this.Project = project;
            this.AdditionalSyntaxTrees = additionalSyntaxTrees.IsDefault ? ImmutableArray<IntroducedSyntaxTree>.Empty : additionalSyntaxTrees;

            this.AdditionalCompilationOutputFiles = additionalCompilationOutputFiles.IsDefault
                ? ImmutableArray<AdditionalCompilationOutputFile>.Empty
                : additionalCompilationOutputFiles;

            this.Transformations = transformations.IsDefault ? ImmutableArray<ITransformationBase>.Empty : transformations;

#if DEBUG
            if ( this.AdditionalSyntaxTrees.GroupBy( x => x.Name ).Any( g => g.Count() > 1 ) )
            {
                throw new AssertionFailedException( "Duplicate item in AdditionalSyntaxTrees." );
            }
#endif
        }

        public AspectPipelineResult WithAdditionalDiagnostics( ImmutableUserDiagnosticList diagnostics )
            => new(
                this.Compilation,
                this.Project,
                this.AspectLayers,
                this.FirstCompilationModel,
                this.LastCompilationModel,
                this.Diagnostics.Concat( diagnostics ),
                this.AspectSources,
                this.ValidatorSources,
                this.ExternallyInheritableAspects,
                this.ReferenceValidators,
                this.AdditionalSyntaxTrees,
                this.AspectInstanceResults,
                this.AdditionalCompilationOutputFiles,
                this.Transformations );
    }
}