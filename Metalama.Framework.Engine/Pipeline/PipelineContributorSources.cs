// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AspectOptions;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Validation;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline
{
    internal sealed record PipelineContributorSources(
        ImmutableArray<IAspectSource> AspectSources,
        ImmutableArray<IValidatorSource> ValidatorSources,
        ImmutableArray<IConfiguratorSource> ConfiguratorSources )
    {
        public static PipelineContributorSources Empty { get; } = new(
            ImmutableArray<IAspectSource>.Empty,
            ImmutableArray<IValidatorSource>.Empty,
            ImmutableArray<IConfiguratorSource>.Empty );

        public PipelineContributorSources Add( PipelineContributorSources other )
        {
            return new PipelineContributorSources(
                this.AspectSources.AddRange( other.AspectSources ),
                this.ValidatorSources.AddRange( other.ValidatorSources ),
                this.ConfiguratorSources.AddRange( other.ConfiguratorSources ) );
        }
    }
}