// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

        public ImmutableArray<ReferenceValidatorInstance> ExternallyVisibleValidators { get; }

        /// <summary>
        /// Gets the compilation model corresponding to <see cref="Compilation"/>, if it has been created.
        /// </summary>
        public ImmutableArray<CompilationModel> CompilationModels { get; }

        internal ImmutableArray<AspectInstanceResult> AspectInstanceResults { get; }

        public IReadOnlyList<IAspectInstance> AspectInstances => this.AspectInstanceResults.SelectArray( x => x.AspectInstance );

        public ImmutableArray<AdditionalCompilationOutputFile> AdditionalCompilationOutputFiles { get; }

        public ImmutableArray<ITransformationBase> Transformations { get; }

        internal AspectPipelineResult(
            PartialCompilation compilation,
            ProjectModel project,
            ImmutableArray<OrderedAspectLayer> aspectLayers,
            ImmutableArray<CompilationModel> compilationModels,
            ImmutableUserDiagnosticList? diagnostics = null,
            ImmutableArray<IAspectSource> aspectSources = default,
            ImmutableArray<IValidatorSource> validatorSources = default,
            ImmutableArray<IAspectInstance> inheritableAspectInstances = default,
            ImmutableArray<ReferenceValidatorInstance> externallyVisibleValidators = default,
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
            this.CompilationModels = compilationModels;
            this.AspectInstanceResults = aspectInstanceResults.IsDefault ? ImmutableArray<AspectInstanceResult>.Empty : aspectInstanceResults;
            this.ExternallyInheritableAspects = inheritableAspectInstances.IsDefault ? ImmutableArray<IAspectInstance>.Empty : inheritableAspectInstances;

            this.ExternallyVisibleValidators =
                externallyVisibleValidators.IsDefault ? ImmutableArray<ReferenceValidatorInstance>.Empty : externallyVisibleValidators;

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
                this.CompilationModels,
                this.Diagnostics.Concat( diagnostics ),
                this.AspectSources,
                this.ValidatorSources,
                this.ExternallyInheritableAspects,
                this.ExternallyVisibleValidators,
                this.AdditionalSyntaxTrees,
                this.AspectInstanceResults,
                this.AdditionalCompilationOutputFiles,
                this.Transformations );
    }
}