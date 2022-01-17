// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Validation;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Aspects
{
    internal interface ITransitiveAspectsManifest
    {
        IEnumerable<string> InheritableAspectTypes { get; }

        IEnumerable<InheritableAspectInstance> GetInheritedAspects( string aspectType );

        ImmutableArray<TransitiveValidatorInstance> Validators { get; }
    }
}