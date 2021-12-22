// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.Validation;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Fabrics
{
    internal record FabricsConfiguration( ImmutableArray<IAspectSource> AspectSources, ImmutableArray<IValidatorSource> ValidatorSources );
}