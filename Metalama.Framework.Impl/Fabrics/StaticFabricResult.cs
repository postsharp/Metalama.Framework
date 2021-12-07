// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Aspects;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Fabrics
{
    internal record StaticFabricResult( ImmutableArray<IAspectSource> AspectSources );
}