// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Engine.Validation;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline
{
    internal sealed record PipelineContributorSources(
        ImmutableArray<IAspectSource> AspectSources,
        ImmutableArray<IValidatorSource> ValidatorSources,
        ImmutableArray<IHierarchicalOptionsSource> OptionsSources,
        IExternalHierarchicalOptionsProvider? ExternalOptionsProvider = null,
        IExternalAnnotationProvider? ExternalAnnotationProvider = null )
    {
        public static PipelineContributorSources Empty { get; } = new(
            ImmutableArray<IAspectSource>.Empty,
            ImmutableArray<IValidatorSource>.Empty,
            ImmutableArray<IHierarchicalOptionsSource>.Empty );

        public PipelineContributorSources Add( PipelineContributorSources other )
        {
            return new PipelineContributorSources(
                this.AspectSources.AddRange( other.AspectSources ),
                this.ValidatorSources.AddRange( other.ValidatorSources ),
                this.OptionsSources.AddRange( other.OptionsSources ),
                this.ExternalOptionsProvider ?? other.ExternalOptionsProvider,
                this.ExternalAnnotationProvider ?? other.ExternalAnnotationProvider );
        }
    }
}