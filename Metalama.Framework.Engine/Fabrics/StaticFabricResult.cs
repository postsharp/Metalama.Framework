// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AspectConfiguration;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Validation;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Fabrics
{
    internal sealed record StaticFabricResult(
        ImmutableArray<IAspectSource> AspectSources,
        ImmutableArray<IValidatorSource> ValidatorSources,
        ImmutableArray<IConfiguratorSource> ConfiguratorSources )
    {
        public static StaticFabricResult Empty { get; } = new(
            ImmutableArray<IAspectSource>.Empty,
            ImmutableArray<IValidatorSource>.Empty,
            ImmutableArray<IConfiguratorSource>.Empty );
    }
}