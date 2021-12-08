// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.AspectOrdering;
using Metalama.Framework.Impl.Sdk;
using System.Collections.Immutable;

namespace Metalama.Framework.Impl.Pipeline
{
    /// <summary>
    /// The static configuration of a <see cref="Metalama.Framework.Impl.Pipeline.PipelineStage"/>.
    /// </summary>
    internal record PipelineStageConfiguration( PipelineStageKind Kind, ImmutableArray<OrderedAspectLayer> Parts, IAspectWeaver? Weaver );
}