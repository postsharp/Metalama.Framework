// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.AspectWeavers;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// The static configuration of a <see cref="Metalama.Framework.Engine.Pipeline.PipelineStage"/>.
    /// </summary>
    internal record PipelineStageConfiguration( PipelineStageKind Kind, ImmutableArray<OrderedAspectLayer> Parts, IAspectWeaver? Weaver );
}