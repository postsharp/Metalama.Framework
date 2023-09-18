// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.HierarchicalOptions;
using Metalama.Framework.Engine.Validation;
using Metalama.Framework.Options;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects
{
    public interface ITransitiveAspectsManifest
    {
        IEnumerable<string> InheritableAspectTypes { get; }

        IEnumerable<InheritableAspectInstance> GetInheritableAspects( string aspectType );

        ImmutableArray<TransitiveValidatorInstance> ReferenceValidators { get; }

        ImmutableDictionary<HierarchicalOptionsKey, IHierarchicalOptions> InheritableOptions { get; }
    }
}