using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Sdk;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.Pipeline
{
    internal record AspectPipelineStageConfiguration( AspectPipelineStageKind Kind, ImmutableArray<OrderedAspectLayer> Parts, IAspectWeaver? Weaver );
}