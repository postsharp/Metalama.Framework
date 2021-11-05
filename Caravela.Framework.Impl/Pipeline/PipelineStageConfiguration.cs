// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Sdk;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The static configuration of a <see cref="PipelineStage"/>.
    /// </summary>
    internal record PipelineStageConfiguration( PipelineStageKind Kind, ImmutableArray<OrderedAspectLayer> Parts, IAspectWeaver? Weaver );
}